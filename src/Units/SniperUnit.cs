using Godot;

public partial class SniperUnit : Unit
{
	public override UnitType type { get; } = UnitType.SNIPER;
	/**
	 * Initializes a Sniper unit. Must be added to the game board with MapController.AddUnit.
	 */
	public SniperUnit(int owner) : base(owner) {
		LoadFromData("sniper");
	}
}
