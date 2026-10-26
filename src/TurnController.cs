using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
	private readonly float aiTurnDelay = 1.0f;
	private int? lastHumanPlayer;
	public int actionCount = 0;
	public readonly int MAX_ACTIONS = 3;
	public int viewingPlayer {
		get { 
			int current = state.currentPlayer;
			if (humanPlayers.Count == 0 || humanPlayers.Contains(current)) return current;
			return lastHumanPlayer ?? humanPlayers.Min();
			
		}
	}

	/** True while the AI planner is running on a worker thread. */
	public bool aiThinking { get; private set; }

	/**
	 * True when the game is waiting for a human to act. Human turns start without delay,
	 * so this is already correct when the EndTurnCommand fires StateChanged.
	 */
	public bool awaitingHuman => humanPlayers.Contains(state.currentPlayer);

	public TurnController() {
		instance = this;
		Name = "TurnController";
	}

	public void init(GameState state, CommandExecutor executor, IEnumerable<int> humanPlayerIDs) {
		this.state = state;
		this.executor = executor;
		humanPlayers.Clear();
		humanPlayers.UnionWith(humanPlayerIDs);
		lastHumanPlayer = null;
	}

	public bool isHuman(int playerID) => humanPlayers.Contains(playerID);

	public IReadOnlyCollection<int> humanPlayerIDs => humanPlayers;

	/** Call once after setup; the initial player's beginTurn upkeep is setup's job. */
	public void startGame() => runCurrentTurn();

	/** Called by the UI (end turn button / hotkey). Ignored when it isn't a human's turn. */
	public void endHumanTurn() {
		if (!awaitingHuman) return;
		lastHumanPlayer = state.currentPlayer;
		endTurn(state.currentPlayer);
	}

	/* ==================== Turn loop ==================== */
	
	private async void runCurrentTurn() {
		if (state.gameOver) return;
		
		try {
			int playerID = state.currentPlayer;
			bool isHuman = humanPlayers.Contains(playerID);
			actionCount = 0;

			if (!isHuman && aiTurnDelay > 0) {
				await ToSignal(GetTree().CreateTimer(aiTurnDelay), SceneTreeTimer.SignalName.Timeout);
				if (!IsInstanceValid(this)) return;
			}
			
			GD.Print($"\n======== Turn {state.turnNumber} ========");
			GD.Print(state.ToString());

			if (isHuman) {
				GD.Print($"Waiting for human player {playerID}");
				return;
			}

			startAITurn(playerID);
		} catch (Exception e) {
			GD.PushError($"Turn loop failed: {e}");
		}
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
		aiThinking = false;

		// The turn may have moved on (e.g. a game was loaded mid-plan); drop stale results.
		if (state.currentPlayer != playerID) return;

		GD.Print($"Applying {commands.Count} commands for player {playerID}");
		foreach (Command cmd in commands) {
			executor.submit(cmd);
		}

		endTurn(playerID);
	}
}
