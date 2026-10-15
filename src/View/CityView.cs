using Godot;

/** City texture fitted inside its cell, with an owner-framed HP bar above it. Drawn beneath units. */
public partial class CityView : EntityView
{
	/** Fraction of the tile's bounding box the city art may fill. Use 1 if your art is already tile-sized. */
	private const float CityFill = 0.9f;

	public override ViewLayer layer => ViewLayer.Cities;

	private Texture2D texture;
	private Color ownerColor;
	private Rect2 drawRect;

	public override void sync(GameState s, HexLayout layout) {
		City c = s.getCity(entityID);
		if (c == null) return;

		Visible = !c.isDead();
		Position = layout.cellToWorld(c.pos);

		texture = ViewTextures.city(c);
		ownerColor = PlayerColors.get(c.owner);
		drawRect = HexLayout.fitRect(texture, layout.cellSize);

		QueueRedraw();
	}

	public override void _Draw() {
		if (texture != null) {
			DrawTextureRect(texture, drawRect, false);
		}
	}
}
