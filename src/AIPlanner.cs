
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class LegalActions
{
	private static readonly Func<GameState, int, IEnumerable<Command>>[] generators =
	{
			moves,
			attacks,
			spawns,
			settles,
	};

	public static List<Command> forPlayer(GameState s, int playerID) {
		var legal = new List<Command>();
		foreach (var generate in generators) {
			foreach (Command cmd in generate(s, playerID)) {
				if (cmd.validate(s)) {
					legal.Add(cmd);
				} else {
					Debug.Print($"Generated command failed validation: {cmd.GetType().Name} " +
					            $"(actor {cmd.actorID}, subject {cmd.subjectID})");
				}
			}
		}
		return legal;
	}

	private static IEnumerable<Unit> activeUnits(GameState s, int playerID) =>
			UnitRules.unitsOf(s, playerID).Where(u => u.currentAP > 0);

	private static IEnumerable<Command> moves(GameState s, int playerID) {
		foreach (Unit u in activeUnits(s, playerID)) {
			foreach (var target in MapRules.movableCells(s, u)) {
				yield return new MoveCommand { actorID = playerID, subjectID = u.id, target = target.Key };
			}		
		}
	}

	private static IEnumerable<Command> attacks(GameState s, int playerID) {
		foreach (Unit u in activeUnits(s, playerID)) {
			if (u.currentAP < u.attackCost) continue;
			foreach (var target in UnitRules.getAttackableCells(s, u))
				yield return new AttackCommand { actorID = playerID, subjectID = u.id, target = target };
		}
	}

	private static IEnumerable<Command> spawns(GameState s, int playerID) {
		foreach (City c in s.allCities().Where(c => c.owner == playerID))
		foreach (UnitDef def in UnitDefs.all())
			yield return new SpawnUnitCommand
			{
					actorID = playerID, subjectID = c.id, uType = def.type, pos = c.gridPosition
			};
	}

	private static IEnumerable<Command> settles(GameState s, int playerID) {
		foreach (Unit u in activeUnits(s, playerID).OfType<SettlerUnit>())
			yield return new SettleCommand { actorID = playerID, subjectID = u.id };
	}
}

public static class AIPlanner
{
	/** Test planner: takes one random legal action, then the turn loop ends the turn. */
	public static List<Command> planTurn(GameState snapshot, int playerID) {
		var plan = new List<Command>();
		List<Command> legal = LegalActions.forPlayer(snapshot, playerID);
		if (legal.Count == 0) return plan;

		List<Command> settles = legal.Where(c => c is SettleCommand).ToList();
		List<Command> spawn = legal.Where(c => c is SpawnUnitCommand).ToList();
		List<Command> attack = legal.Where(c => c is AttackCommand).ToList();

		Command pick;
		
		if (settles.Count > 0) {
			pick = settles[0];
		} else if (spawn.Count > 0) {
			pick = spawn[0];
		} else if (attack.Count > 0) {
			pick = attack[0];
		} else {
			pick = legal[snapshot.rng.Next(legal.Count)];
		}
		
		// Apply to the snapshot so a multi-action planner sees its own earlier choices.
		pick.execute(snapshot);
		plan.Add(pick);
		return plan;
	}
}

