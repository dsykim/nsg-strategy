using Godot;
using System.Text.Json.Nodes;

public partial class SettlerUnit : Unit
{
	public SettlerUnit(int owner) : base(owner) {
		LoadFromData("settler");
	}
	
}
