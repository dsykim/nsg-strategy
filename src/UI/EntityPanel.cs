using Godot;
using System.Collections.Generic;

/**
 * A HudWidget that shows the selected entity: visibility follows the selection,
 * the shared stats (name, owner, health) refresh here, and action buttons are built
 * from PlayerActions.forEntity and routed through InputController.invoke — the same
 * path as the hotkeys.
 *
 * Subclasses say which entities they show and fill in only their extra stats.
 */
public abstract partial class EntityPanel : HudWidget
{
	[Export] private Label nameLabel;
	[Export] private Label ownerLabel;
	[Export] private Label hpLabel;
	[Export] private Range hpBar;   // ProgressBar or TextureProgressBar
	[Export] protected Container actionButtonContainer;

	private int boundID;
	private readonly List<(PlayerAction action, Button button)> buttons = new();

	/** Whether this panel displays `e` (e.g. UnitPanel shows Units). */
	protected abstract bool shows(CellDecorator e);

	/** Fill panel-specific labels/bars. Shared stats are already done. */
	protected virtual void refreshStats(CellDecorator e) { }

	/* ==================== Lifecycle ==================== */

	protected override void onSubscribe() {
		ctx.selection.Changed += onSelectionChanged;
		ctx.input.ModeChanged += refreshButtons;
		Visible = false;
		onSelectionChanged();
	}

	protected override void onUnsubscribe() {
		ctx.selection.Changed -= onSelectionChanged;
		ctx.input.ModeChanged -= refreshButtons;
	}

	protected override void refresh() {
		if (boundID == 0) return;
		CellDecorator e = ctx.state.getEntity(boundID);
		if (e == null) return;

		if (e is PlayerDecorator pd) {
			setText(nameLabel, capitalize(pd.displayName));
			setText(ownerLabel, $"Player {pd.owner}");
		}
		if (e is IHasHealth h) {
			setText(hpLabel, $"{h.currentHP} / {h.maxHP}");
			setBar(hpBar, h.currentHP, h.maxHP);
		}

		refreshStats(e);
		refreshButtons();
	}

	/* ==================== Binding ==================== */

	private void onSelectionChanged() {
		CellDecorator e = ctx.selection.resolve(ctx.state);
		if (e == null || !shows(e)) {
			boundID = 0;
			clearButtons();
			Visible = false;
			return;
		}

		if (e.id != boundID) {
			boundID = e.id;
			rebuildButtons(e);
		}

		Visible = true;
		refresh();
	}

	/* ==================== Action buttons ==================== */

	private void rebuildButtons(CellDecorator e) {
		clearButtons();
		if (actionButtonContainer == null) return;

		// Only entities owned by a human get buttons; AI entities are view-only.
		if (e is not PlayerDecorator pd || !TurnController.instance.isHuman(pd.owner)) return;

		foreach (PlayerAction a in PlayerActions.forEntity(e)) {
			var button = new Button
			{
					Text = a.label,
					TooltipText = InputBindings.hint(a.inputAction),
					ToggleMode = a is TargetedAction,   // shows "pressed" while waiting for a target
					FocusMode = FocusModeEnum.None,     // never steal hotkeys or Enter (end turn)
			};
			button.AddThemeFontSizeOverride("font_size", 24);
			button.Pressed += () => {
				ctx.input.invoke(a);
				refreshButtons(); // undo the toggle if the invoke was rejected
			};
			actionButtonContainer.AddChild(button);
			buttons.Add((a, button));
		}
	}

	private void clearButtons() {
		foreach (var (_, button) in buttons) button.QueueFree();
		buttons.Clear();
	}

	private void refreshButtons() {
		foreach (var (action, button) in buttons) {
			bool active = action == ctx.input.targetingAction;
			button.Disabled = !active && !ctx.input.canInvoke(action);
			if (button.ToggleMode) button.SetPressedNoSignal(active);
		}
	}
}
