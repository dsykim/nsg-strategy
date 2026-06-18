using Godot;
using System.Diagnostics;
using System.Numerics;

public class CombatController
{
	public static CombatController instance { get; private set; }
	public CombatController() {
		instance = this;
	}
	
	public void resolveCombat(Unit attacker, Vector2I defenderPos) {
		Debug.Print("Combat resolved");
		
	}
}
