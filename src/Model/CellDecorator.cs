using Godot;

public abstract class CellDecorator
{
	public Vector2I gridPosition;
	public int id { get; private set; } = 0;

	public void assignId(int newId) {
		if (id != 0) {
			GD.PrintErr($"Entity already has id {id}, refusing to reassign");
			return;
		}
		id = newId;
	}

	public abstract CellDecorator clone();
}
