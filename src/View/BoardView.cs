using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Root of the board's visuals. The only view node that knows about CommandExecutor.
 *
 * On StateChanged it marks itself dirty; once per frame it reconciles entity views against
 * state.entities (create new IDs, sync existing ones, free missing ones) and redraws terrain.
 * Several commands landing in one frame therefore cost one sync.
 *
 * Strictly downstream of the model: nothing here writes to GameState, and the game runs
 * identically when no BoardView exists (headless).
 */
public partial class BoardView : Node2D
{
	/** Tile size comes from this terrain's texture, so tile art lines up pixel-for-pixel. */
	[Export] public TerrainType referenceTerrain = TerrainType.PLAINS;

	/** Non-zero overrides the texture-derived tile size (e.g. if your art has transparent padding). */
	[Export] public Vector2 cellSizeOverride = Vector2.Zero;

	private static readonly Vector2 fallbackCellSize = new Vector2(32f, 37f);

	public HexLayout layout { get; private set; }

	/** Pixel size of the drawn board including margin; for a future camera to fit or clamp to. */
	public Vector2 boardSize { get; private set; }

	/** Hover / selection / target outlines. Purely visual; driven by the input layer. */
	public BoardOverlays overlays { get; private set; }

	private GameState state;
	private CommandExecutor executor;
	private TurnController turns;

	private TerrainLayer terrain;
	private readonly Dictionary<ViewLayer, Node2D> layers = new();
	public readonly Dictionary<int, EntityView> views = new();
	private readonly HashSet<int> seen = new();
	private bool dirty;
	private int viewer => turns?.viewingPlayer ?? state.currentPlayer;

	public BoardView() {
		Name = "BoardView";
	}

	/** Bind to a state and the executor that mutates it. Safe to call again (e.g. after a load). */
	public void init(GameState s, CommandExecutor ex) {
		ensureLayers();

		if (executor != null) executor.StateChanged -= onStateChanged;
		executor = ex;
		executor.StateChanged += onStateChanged;

		turns = TurnController.instance;
		
		rebuild(s);
	}

	/** Throw away all entity views and rebuild from a state. */
	public void rebuild(GameState s) {
		state = s;

		foreach (EntityView v in views.Values) v.QueueFree();
		views.Clear();

		layout = buildLayout(s.grid);
		terrain.bind(s, layout);
		overlays.bind(layout);
		syncNow();
	}

	/** The in-grid cell under the mouse, if any. */
	public bool tryGetCellAtMouse(out Vector2I cell) {
		cell = default;
		if (state == null || layout == null) return false;
		cell = layout.worldToCell(GetLocalMousePosition());
		return state.grid.indexInGrid(cell);
	}

	public override void _Process(double delta) {
		if (!dirty) return;
		dirty = false;
		syncNow();
	}

	public override void _ExitTree() {
		if (executor != null) executor.StateChanged -= onStateChanged;
	}

	private void onStateChanged() => dirty = true;

	private void syncNow() {
		if (state == null) return;
		seen.Clear();
		
		var visible = state.getPlayer(turns.viewingPlayer).visibleCells;

		foreach (var (id, entity) in state.entities) {
			if (!views.TryGetValue(id, out EntityView view)) {
				view = EntityViewFactory.create(entity);
				if (view == null) continue;

				view.bindEntity(id);
				layers[view.layer].AddChild(view);
				views[id] = view;
			}

			seen.Add(id);
			view.sync(state, layout);
			if (state.fogOfWar && view.hiddenByFog) view.Visible = visible.Contains(entity.pos);
		}

		if (views.Count > seen.Count) {
			foreach (int id in views.Keys.Where(id => !seen.Contains(id)).ToList()) {
				views[id].QueueFree();
				views.Remove(id);
			}
		}
		
		overlays.setBorderRegion(state);

		terrain.sync();
	}

	private void ensureLayers() {
		if (terrain != null) return;

		terrain = new TerrainLayer { Name = "TerrainLayer" };
		AddChild(terrain);

		// Enum order is draw order.
		foreach (ViewLayer l in Enum.GetValues<ViewLayer>()) {
			var node = new Node2D { Name = $"{l}Layer" };
			AddChild(node);
			layers[l] = node;
		}

		// Last child: outlines draw above terrain and entities.
		overlays = new BoardOverlays();
		AddChild(overlays);
	}

	private Vector2 resolveCellSize() {
		if (cellSizeOverride != Vector2.Zero) return cellSizeOverride;

		Texture2D reference = ViewTextures.terrain(referenceTerrain);
		if (reference != null) return reference.GetSize();

		GD.PushWarning($"No texture for {referenceTerrain}; using fallback cell size {fallbackCellSize}");
		return fallbackCellSize;
	}

	/** Lay out with origin at zero, then shift so the whole board sits inside a half-tile margin. */
	private HexLayout buildLayout(HexGrid grid) {
		var raw = new HexLayout(resolveCellSize(), Vector2.Zero);

		var min = new Vector2(float.MaxValue, float.MaxValue);
		var max = new Vector2(float.MinValue, float.MinValue);
		foreach (HexCell cell in grid.grid) {
			Vector2 p = raw.cellToWorld(cell.pos);
			min = min.Min(p);
			max = max.Max(p);
		}

		Vector2 margin = raw.cellSize / 2f;
		boardSize = max - min + margin * 2f;
		return raw.withOrigin(margin - min);
	}
}
