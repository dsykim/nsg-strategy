using Godot;

public partial class MeleeUnit : Unit
{
	/**
	 * Initializes a melee unit. Must be added to the game board with MapController.AddUnit.
	 */
	public MeleeUnit(int owner) : base(owner) {
		LoadFromData("melee");
	}
}
