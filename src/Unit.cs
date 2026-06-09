using Godot;
		
public abstract partial class Unit : Sprite2D
{
	public int hp;
	public int actionPoints;
	public int currentAP;
	public Vector2I gridPosition;

	public readonly int owner;

	public Unit(int owner) {
		this.owner = owner;
	}

	
	public void move(Vector2I target) {
		currentAP -= HexGrid.hexDistance(gridPosition, target);
		gridPosition = target;
	}

	public void setCurrentAP(int val) {
		currentAP = val;
	}
}
