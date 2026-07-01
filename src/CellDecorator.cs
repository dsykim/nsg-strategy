using Godot;

public abstract partial class CellDecorator : Sprite2D
{
	public Vector2I gridPosition;
	public int id { get; private set; } = 0;

	public void assignId(int newId) {
		if (id != 0) {
			GD.PrintErr($"Entity {Name} already has id {id}, refusing to reassign");
			return;
		}
		id = newId;
}
}
