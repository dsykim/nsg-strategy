using Godot;
		
public abstract partial class Unit : Sprite2D
{
	public int hp { get; protected set; }
	public int actionPoints { get; protected set; }
	public int currentAP { get; protected set; }
	public Vector2I gridPosition;

	public readonly int owner;

	public Unit(int owner) {
		this.owner = owner;
	}

	
	public void move(Vector2I target) {
		currentAP -= HexGrid.hexDistance(gridPosition, target);
		gridPosition = target;
	}
	
}
