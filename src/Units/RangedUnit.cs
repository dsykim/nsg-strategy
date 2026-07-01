using Godot;

public partial class RangedUnit : Unit
{
	public override UnitType type { get; } = UnitType.RANGED;
	/**
	 * Initializes a ranged unit. Must be added to the game board with MapController.AddUnit.
	 */
	public RangedUnit(int owner) : base(owner) {
		LoadFromData("ranged");
	}
}
