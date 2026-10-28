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
			if (u.hasAttacked) continue;
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

	private static bool isScoreGood(float score, Dictionary<Sequence, (float, GameState)> scoreDict, int maxEntries) {
		return scoreDict.Count < maxEntries || score > scoreDict.Min(p => p.Value.Item1);
	}
	
	public static List<Command> planTurn(GameState snapshot, int playerID) {
		snapshot.actionCount = 0;
		List<Command> legal = LegalActions.forPlayer(snapshot, playerID);
		if (legal.Count == 0) return new Sequence();

		List<Sequence> sequencesToEvaluate = new List<Sequence>();
		Dictionary<Sequence, (float, GameState)> scoreDict = new Dictionary<Sequence, (float, GameState)>();
		foreach (var c in legal) sequencesToEvaluate.Add(new Sequence {c});

		int stepsEvaluated = 0;
		int MAX_FORWARD_SEQUENCES = 5;
		
		while (stepsEvaluated < snapshot.MAX_ACTIONS) {
			if (sequencesToEvaluate.Count == 0) break;
			scoreDict.Clear();

			foreach (Sequence seq in sequencesToEvaluate) {
				GameState newState = applySequence(snapshot.clone(), seq);
				float score = evaluateDelta(snapshot, newState);
				if (isScoreGood(score, scoreDict, MAX_FORWARD_SEQUENCES)) {
					// Evaluate incoming sequences, prune down to best M before forward sim
					scoreDict[seq] = (score, newState);
					if (scoreDict.Count > MAX_FORWARD_SEQUENCES) {
						scoreDict.Remove(scoreDict.MinBy(p=>p.Value.Item1).Key);
					}
				}
			}
			sequencesToEvaluate.Clear();
			if (stepsEvaluated < snapshot.MAX_ACTIONS - 1) {
				bool anyExpanded = false;
				// if not last step, forward sim on best M sequences
				foreach (var p in scoreDict) {
					Sequence seq = p.Key;
					GameState state = p.Value.Item2;
					List<Command> next = LegalActions.forPlayer(state, playerID);
					if (next.Count == 0) {
						// Terminal sequence, pass forward for evaluation
						sequencesToEvaluate.Add(seq);
						continue;
					}
					anyExpanded = true;
					foreach (Command c in next) {
						Sequence newSequence = new Sequence(seq);
						newSequence.Add(c);
						sequencesToEvaluate.Add(newSequence);
					}
				}
				if (!anyExpanded) break; // every plan is terminal
			}
			
			stepsEvaluated++;
		}
		// last step, take the best evaluated sequence or empty if none found
		return scoreDict.Count > 0 ? scoreDict.MaxBy(p => p.Value.Item1).Key : new Sequence();
	}
}

