using Godot;
using System.Text.Json.Nodes;

public partial class SettlerUnit : Unit
{
	public override UnitType type { get; } = UnitType.SETTLER;
	public SettlerUnit(int owner) : base(owner) {
		LoadFromData("settler");
	}
	
}
