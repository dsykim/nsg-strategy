using Godot;

/**
 * Draws each cell's terrain texture into its tile rect, plus a tint for cells a player controls.
 * Redraws the whole grid on sync. Every cell of a terrain type uses the same texture, so the
 * renderer batches these; at ~1000 cells it's cheap enough not to bother diffing.
 */
public partial class TerrainLayer : Node2D
{
	private const float ControllerTintAlpha = 0.28f;
	private readonly Color hiddenColor = new Color(0.2f, 0.2f, 0.2f, 0.35f);

	private GameState state;
	private HexLayout layout;

	public void bind(GameState s, HexLayout l) {
		state = s;
		layout = l;
		QueueRedraw();
	}

	public void sync() => QueueRedraw();

	public override void _Draw() {
		if (state == null || layout == null) return;
		
		foreach (HexCell cell in state.grid.grid) {
			Vector2 center = layout.cellToWorld(cell.pos);

			Texture2D tex = ViewTextures.terrain(cell.terrainType);
			if (tex != null) DrawTextureRect(tex, layout.cellRect(center), false);

			int viewingPlayer = TurnController.instance.viewingPlayer;
			if (state.fogOfWar && !state.getPlayer(viewingPlayer).visibleCells.Contains(cell.pos)) {
				// Fog of war
				DrawColoredPolygon(layout.corners(center), hiddenColor);
			} else if (cell.hasController()) {
				// DrawColoredPolygon(layout.corners(center),
				// 		new Color(PlayerColors.get(cell.controllerID), ControllerTintAlpha));
			}
		}
	}
}
