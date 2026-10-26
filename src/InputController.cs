using Godot;
using System;
using System.Collections.Generic;

/**
 * Translates mouse and keyboard input into selection changes and Commands.
 * Never writes to GameState: every change goes through CommandExecutor.submit,
 * the same path the AI uses.
 *
 * Modes:
 *   Idle      — clicks select, hotkeys invoke actions on the selection.
 *   Targeting — a TargetedAction is waiting for a cell; valid cells are outlined.
 *               Click a valid cell to confirm; invalid click / right-click / Escape cancels.
 *
 * Panels call canInvoke/invoke/endTurn so buttons and hotkeys share one code path.
 */
public partial class InputController : Node
{
	private GameState state;
	private CommandExecutor executor;
	private BoardView board;
	private Selection selection;
	private TurnController turns;

	/** In-grid cell under the mouse, or null. */
	public Vector2I? hoveredCell { get; private set; }

	/** The action waiting for a target cell, or null when idle. */
	public TargetedAction targetingAction { get; private set; }
	public bool isTargeting => targetingAction != null;
	private int targetingEntityID;
	private readonly HashSet<Vector2I> validTargets = new();
	private readonly HashSet<Vector2I> cellsInRange = new();

	/** Fired when targeting starts or ends (e.g. so a panel can highlight the active button). */
	public event Action ModeChanged;

	public InputController() {
		Name = "InputController";
	}

	public void init(GameState s, CommandExecutor ex, BoardView b, Selection sel) {
		unsubscribe();
		InputBindings.ensureDefaults();

		state = s;
		executor = ex;
		board = b;
		selection = sel;
		turns = TurnController.instance;

		executor.StateChanged += onStateChanged;
		selection.Changed += onSelectionChanged;
		refreshSelectionHighlight();
	}

	public override void _ExitTree() => unsubscribe();

	private void unsubscribe() {
		if (executor != null) executor.StateChanged -= onStateChanged;
		if (selection != null) selection.Changed -= onSelectionChanged;
	}

	/* ==================== Public API (hotkeys and panels) ==================== */

	/** True if the current player is a human whose turn it is and who owns `e`. */
	public bool canControl(CellDecorator e) =>
			e is PlayerDecorator pd && turns.awaitingHuman && pd.owner == state.currentPlayer;

	public bool canInvoke(PlayerAction a) {
		CellDecorator e = selection.resolve(state);
		return e != null && canControl(e) && a.appliesTo(e) && a.isAvailable(state, state.currentPlayer, e);
	}

	public void invoke(PlayerAction a) {
		// Pressing the active targeted action again toggles it off.
		if (a == targetingAction) {
			cancelTargeting();
			return;
		}

		if (!canInvoke(a)) {
			GD.Print($"{a.label} not available");
			return;
		}

		CellDecorator e = selection.resolve(state);
		switch (a) {
			case InstantAction instant:
				cancelTargeting();
				executor.submit(instant.build(state, state.currentPlayer, e));
				break;
			case TargetedAction targeted:
				beginTargeting(targeted, e);
				break;
		}
	}

	public void endTurn() {
		cancelTargeting();
		turns.endHumanTurn();
	}

	/* ==================== Input ==================== */

	public override void _Process(double delta) => updateHover();

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseButton mb && mb.Pressed) {
			if (mb.ButtonIndex == MouseButton.Left) {
				if (isTargeting) handleTargetClick();
				else handleSelectClick();
				GetViewport().SetInputAsHandled();
			} else if (mb.ButtonIndex == MouseButton.Right) {
				cancelOrDeselect();
				GetViewport().SetInputAsHandled();
			}
			return;
		}

		if (!@event.IsPressed() || @event.IsEcho()) return;

		if (@event.IsActionPressed("ui_cancel")) {
			cancelOrDeselect();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed(InputBindings.EndTurn)) {
			endTurn();
			GetViewport().SetInputAsHandled();
			return;
		}

		foreach (PlayerAction a in PlayerActions.forEntity(selection.resolve(state))) {
			if (a.inputAction != null && @event.IsActionPressed(a.inputAction)) {
				invoke(a);
				GetViewport().SetInputAsHandled();
				return;
			}
		}
	}

	private void updateHover() {
		Vector2I? next = board.tryGetCellAtMouse(out Vector2I cell) ? cell : null;
		if (next == hoveredCell) return;
		hoveredCell = next;
		board.overlays.setHover(hoveredCell);
	}

	private void cancelOrDeselect() {
		if (isTargeting) cancelTargeting();
		else selection.clear();
	}

	/**
	 * Clicking cycles through the selectable entities on a cell (unit first, then city).
	 * Clicking a cell with nothing selectable, or off the board, deselects.
	 */
	private void handleSelectClick() {
		if (hoveredCell is not Vector2I pos) {
			selection.clear();
			return;
		}

		List<int> candidates = selectableAt(pos);
		if (candidates.Count == 0) {
			selection.clear();
			return;
		}

		int i = candidates.IndexOf(selection.entityID); // -1 if current selection isn't here
		selection.select(candidates[(i + 1) % candidates.Count]);
	}

	private void handleTargetClick() {
		if (hoveredCell is Vector2I pos && validTargets.Contains(pos)) confirmTarget(pos);
		else cancelTargeting();
	}

	/** Entity IDs on a cell, in click-cycle order. Add new selectable decorators here. */
	private List<int> selectableAt(Vector2I pos) {
		HexCell cell = state.grid.getCell(pos);
		var ids = new List<int>(2);
		if (cell.hasUnit() && board.views[cell.unitID].Visible) ids.Add(cell.unitID);
		if (cell.hasCity(state) && board.views[cell.playerDecoratorID].Visible) ids.Add(cell.playerDecoratorID);
		return ids;
	}

	/* ==================== Targeting ==================== */

	private void beginTargeting(TargetedAction action, CellDecorator e) {
		targetingAction = action;
		targetingEntityID = e.id;
		recomputeTargets(e);
		ModeChanged?.Invoke();
	}

	private void recomputeTargets(CellDecorator e) {
		validTargets.Clear();
		validTargets.UnionWith(targetingAction.validTargets(state, e));
		cellsInRange.Clear();
		cellsInRange.UnionWith(targetingAction.allCellsInRange(state, e));
		board.overlays.showRangeRegion(cellsInRange);
		board.overlays.showTargetRegion(validTargets, targetingAction.highlight);
	}

	private void confirmTarget(Vector2I pos) {
		TargetedAction action = targetingAction;
		CellDecorator e = state.getEntity(targetingEntityID);
		cancelTargeting(); // clear the mode first; submit fires StateChanged synchronously
		if (e != null) executor.submit(action.build(state.currentPlayer, e, pos));
	}

	private void cancelTargeting() {
		if (!isTargeting) return;
		targetingAction = null;
		targetingEntityID = 0;
		validTargets.Clear();
		cellsInRange.Clear();
		board.overlays.clearTargetRegion();
		board.overlays.clearRangeRegion();
		ModeChanged?.Invoke();
	}

	/** Keep an open targeting mode honest after any state change (turn ended, entity died...). */
	private void refreshTargeting() {
		if (!isTargeting) return;
		CellDecorator e = state.getEntity(targetingEntityID);
		if (e == null || !canControl(e) || !targetingAction.isAvailable(state, state.currentPlayer, e)) {
			cancelTargeting();
			return;
		}
		recomputeTargets(e);
	}

	/* ==================== Reactions ==================== */

	private void onStateChanged() {
		// Selection has already revalidated itself (it subscribes first).
		refreshTargeting();
		refreshSelectionHighlight();     // entity may have moved
	}

	private void onSelectionChanged() {
		if (isTargeting && selection.entityID != targetingEntityID) cancelTargeting();
		refreshSelectionHighlight();
	}

	private void refreshSelectionHighlight() {
		CellDecorator e = selection.resolve(state);
		board.overlays.setSelection(e?.pos);
	}
}
