using Godot;

public partial class MeleeUnit : Unit
{
	public override UnitType type { get; } = UnitType.MELEE;
	/**
	 * Initializes a melee unit. Must be added to the game board with MapController.AddUnit.
	 */
	public MeleeUnit(int owner) : base(owner) {
		LoadFromData("melee");
	}
}
