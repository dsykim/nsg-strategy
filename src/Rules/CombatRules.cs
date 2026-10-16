using Godot;
using System.Diagnostics;
using System.Linq;

public static class CombatRules
{
	public static void resolveCombat(GameState state, Unit attacker, Vector2I defenderPos) {
		attacker.spendAP(attacker.attackCost);
		(Unit u, City c) defenders = MapRules.getCellDefenders(state, defenderPos);

		if (defenders.u != null) {
			defenders.u.applyDamage(attacker.damage);
			if (defenders.u.isDead) {
				UnitRules.deleteUnit(state, defenders.u);
			}
		} else if (defenders.c != null) {
			int defenderID = defenders.c.owner;
			defenders.c.applyDamage(attacker.damage);
			if (defenders.c.isDead()) {
				CityRules.deleteCity(state, defenders.c);
				if (!isPlayerAlive(state, defenderID)) {
					state.getPlayer(defenderID).alive = false;
				}
			}
		}
		
		Debug.Print("Combat resolved");
	}

	public static bool isPlayerAlive(GameState s, int pid) {
		if (s.getPlayer(pid).hasSettled) return s.playerCities(pid).Exists(c => c is Capital);
		return s.playerUnits(pid).Exists(u => u is SettlerUnit);
	}
}
