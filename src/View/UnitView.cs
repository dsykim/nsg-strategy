using Godot;

/** Unit texture fitted inside its cell, with an owner-framed HP bar beneath it. */
public partial class UnitView : EntityView
{

	public override ViewLayer layer => ViewLayer.Units;
	public override bool hiddenByFog => true;

	private Texture2D texture;
	private Rect2 drawRect;
	private Color ownerColor;

	public override void sync(GameState s, HexLayout layout) {
		Unit u = s.getUnit(entityID);
		if (u == null) return;

		Visible = !u.isDead;
		Position = layout.cellToWorld(u.pos);
		texture = ViewTextures.unit(u);
		ownerColor = PlayerColors.get(u.owner);
		drawRect = HexLayout.fitRect(texture, layout.cellSize);

		QueueRedraw();
	}

	public override void _Draw() {
		if (texture != null) DrawTextureRect(texture, drawRect, false, ownerColor);
	}
}
