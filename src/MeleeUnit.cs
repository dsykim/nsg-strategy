
using Godot;

public partial class MeleeUnit : Unit
{
	/**
	 * Initializes a melee unit. Must be added to the game board with MapController.AddUnit.
	 */
	public MeleeUnit() {
		hp = 3;
		actionPoints = 3;
		currentAP = actionPoints;
		Texture = ResourceLoader.Load<Texture2D>("res://assets/meleeUnit.png");
	}
}
