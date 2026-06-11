using Godot;

public partial class SettlerUnit : Unit
{
	public SettlerUnit(int owner) : base(owner) {
		maxHP = 20;
		maxAP = 3;
		currentAP = maxAP;
		Texture = ResourceLoader.Load<Texture2D>("res://assets/meleeUnit.png");
	}
	
}
