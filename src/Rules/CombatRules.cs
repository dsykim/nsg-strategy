using Godot;
using System.Diagnostics;
using System.Linq;

public static class CombatRules
{
	public static void resolveCombat(GameState state, Unit attacker, Vector2I defenderPos) {
		attacker.spendAP(attacker.attackCost);
		(Unit u, City c) defenders = MapRules.getCellDefenders(state, defenderPos);

		// TODO: currently only handle combat with units
		if (defenders.u != null) {
			defenders.u.applyDamage(attacker.damage);
			if (defenders.u.isDead) {
				UnitRules.deleteUnit(state, defenders.u);
			}
		} else if (defenders.c != null) {
			int defenderID = defenders.c.owner;
			defenders.c.applyDamage(attacker.damage);
			if (defenders.c.isDead()) {
				if (!isPlayerAlive(state, defenderID)) {
					state.getPlayer(defenderID).alive = false;
				}
				CityRules.deleteCity(state, defenders.c);
			}
		}
		
		Debug.Print("Combat resolved");
	}

	public static bool isPlayerAlive(GameState s, int pid) {
		return s.allCities().Any(u => u.owner == pid);
	}
}
