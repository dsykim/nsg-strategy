using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sequence = System.Collections.Generic.List<Command>;

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
					// Debug.Print($"Generated command failed validation: {cmd.GetType().Name} " +
					//             $"(actor {cmd.actorID}, subject {cmd.subjectID})");
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
			if (!u.hasAttacked) continue;
			foreach (var target in UnitRules.getAttackableCells(s, u))
				yield return new AttackCommand { actorID = playerID, subjectID = u.id, target = target };
		}
	}

	private static IEnumerable<Command> spawns(GameState s, int playerID) {
		foreach (City c in s.allCities().Where(c => c.owner == playerID))
		foreach (UnitDef def in UnitDefs.all())
			yield return new SpawnUnitCommand
			{
					actorID = playerID, subjectID = c.id, uType = def.type, pos = c.pos
			};
	}

	private static IEnumerable<Command> settles(GameState s, int playerID) {
		foreach (Unit u in activeUnits(s, playerID).OfType<SettlerUnit>())
			yield return new SettleCommand { actorID = playerID, subjectID = u.id };
	}
}

public static class AIPlanner
{
	private static GameState applySequence(GameState state, Sequence seq) {
		foreach (Command c in seq) c.execute(state);
		return state;
	}

	private static float evaluateState(GameState state) {
		return 1f;
	}

	private static float evaluateDelta(GameState begin, GameState end) {
		// We shouldn't use an absolute metric to evaluate a plan or else all plans starting from poor states
		// will be rejected, and bad plans from good states will be accepted
		// return evaluateState(end) - evaluateState(begin);
		return 1f;
	}
	
	public static List<Command> planTurn(GameState state, int playerID) {
		state.actionCount = 0;
		List<Command> legal = LegalActions.forPlayer(state, playerID);
		if (legal.Count == 0) return new Sequence();

		List<Sequence> sequencesToEvaluate = new List<Sequence>();
		List<Sequence> successfulSequences = new List<Sequence>();
		List<Sequence> temp = new List<Sequence>();
		foreach (var c in legal) sequencesToEvaluate.Add(new Sequence {c});

		float threshold = 0f;
		int stepsEvaluated = 0;
		while (stepsEvaluated < state.MAX_ACTIONS) {
			foreach (Sequence seq in sequencesToEvaluate) {
				GameState newState = applySequence(state.clone(), seq);
				float score = evaluateDelta(newState, state);
				if (score > threshold) {
					if (stepsEvaluated < state.MAX_ACTIONS - 1) {
						// Forward sim
						foreach (Command c in LegalActions.forPlayer(newState, playerID)) {
							Sequence newSequence = new Sequence(seq);
							newSequence.Add(c);
							temp.Add(newSequence);
						}
					} else {
						// No forward sim needed on last step
						successfulSequences.Add(seq);
					}
				}
			}
			
			sequencesToEvaluate = new List<Sequence>(temp);
			temp.Clear();
			stepsEvaluated++;
		}

		if (successfulSequences.Count == 0) return new Sequence();
		return successfulSequences[0];
	}
}

