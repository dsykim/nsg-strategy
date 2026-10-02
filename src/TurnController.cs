using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/**
 * Orchestrates the turn loop: waits for human input, runs AI planning off the main
 * thread, and submits every state change through the CommandExecutor.
 *
 * Holds no game rules and no game data. Whose turn it is lives in GameState;
 * this class only knows which players are controlled by a human.
 */
public partial class TurnController : Node
{
	public static TurnController instance { get; private set; }

	private GameState state;
	private CommandExecutor executor;
	private readonly HashSet<int> humanPlayers = new();

	public bool aiThinking { get; private set; }

	/** True when the game is waiting for a human to act. UI can gate input on this. */
	public bool awaitingHuman => !aiThinking && humanPlayers.Contains(state.currentPlayer);

	public TurnController() {
		Name = "TurnController";
		instance = this;
	}

	public void init(GameState state, CommandExecutor executor, IEnumerable<int> humanPlayerIDs) {
		this.state = state;
		this.executor = executor;
		humanPlayers.Clear();
		humanPlayers.UnionWith(humanPlayerIDs);
	}

	/** Call once after setup; the initial player's beginTurn upkeep is setup's job. */
	public void startGame() => runCurrentTurn();

	/** Called by the UI (end turn button). Ignored when it isn't a human's turn. */
	public void endHumanTurn() {
		if (!awaitingHuman) return;
		endTurn(state.currentPlayer);
	}

	/* ==================== Turn loop ==================== */

	private void runCurrentTurn() {
		int playerID = state.currentPlayer;

		if (humanPlayers.Contains(playerID)) {
			aiThinking = false;
			GD.Print($"Waiting for human player {playerID}");
			return;
		}

		startAITurn(playerID);
	}

	private void endTurn(int playerID) {
		executor.submit(new EndTurnCommand { actorID = playerID });
		runCurrentTurn();
	}

	/* ==================== AI ==================== */

	private void startAITurn(int playerID) {
		aiThinking = true;

		// Clone on the main thread; the worker only ever touches its own copy.
		GameState snapshot = state.clone();

		Task.Run(() => {
			List<Command> commands;
			try {
				commands = AIPlanner.planTurn(snapshot, playerID);
			} catch (Exception e) {
				GD.PushError($"AI planner failed for player {playerID}: {e}");
				commands = new List<Command>();
			}

			// Hop back to the main thread with the results; no shared fields needed.
			Callable.From(() => onAIFinished(playerID, commands)).CallDeferred();
		});
	}

	private void onAIFinished(int playerID, List<Command> commands) {
		// The turn may have moved on (e.g. a game was loaded mid-plan); drop stale results.
		if (state.currentPlayer != playerID) return;

		GD.Print($"Applying {commands.Count} commands for player {playerID}");
		foreach (Command cmd in commands) {
			executor.submit(cmd);
		}

		endTurn(playerID);
	}
}
