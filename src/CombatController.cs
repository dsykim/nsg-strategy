using Godot;
using System.Diagnostics;
using System.Numerics;

public partial class CombatController : Node
{
	public static CombatController instance { get; private set; }

	[Signal]
	public delegate void CombatResolvedEventHandler();

	public CombatController() {
		instance = this;
	}

	public void resolveCombat(Unit attacker, Vector2I defenderPos) {
		attacker.setCurrentAP(attacker.currentAP - attacker.attackCost);
		MapController map = MapController.instance;
		(Unit u, City c) defenders = map.getCellDefender(defenderPos);

		// TODO: currently only handle combat with units
		if (defenders.u != null) {
			defenders.u.setCurrentHP(defenders.u.currentHP -= attacker.damage);
		}

		EmitSignal(SignalName.CombatResolved);

		Debug.Print("Combat resolved");
	}
}
