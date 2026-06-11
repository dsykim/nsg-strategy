using Godot;

public partial class MeleeUnit : Unit
{
	/**
	 * Initializes a melee unit. Must be added to the game board with MapController.AddUnit.
	 */
	public MeleeUnit(int owner) : base(owner) {
		maxHP = 100;
		maxAP = 3;
		currentAP = maxAP;
		Texture = ResourceLoader.Load<Texture2D>("res://assets/meleeUnit.png");
	}
}
