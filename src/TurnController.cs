using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public partial class TurnController : Node
{
	private readonly int MAX_PLAYER_COUNT = 4;
	private PlayerController userPlayer;
	private List<PlayerController> aiPlayers = new List<PlayerController>();

	private Thread aiThread;
	private Dictionary<string, string> aiResult = new Dictionary<string, string>();
	private bool aiThinking = false;
	
	private int currentPlayer;
	private int playerCount;
	
	private Button nextTurnButton;

	public TurnController() {
		currentPlayer = 0;
	}

	public void init(int playerCount) {
		setPlayerCount(playerCount);
		nextTurnButton = GetNode<Button>("../UI/NextTurnButton");
		nextTurnButton.Pressed += nextTurn;
	}

	public void setPlayerCount(int n) {
		if (n > 1 && n <= MAX_PLAYER_COUNT) {
			playerCount = n;
			userPlayer = new PlayerController(0);
			for (int i = 1; i < n; i++) {
				PlayerController aiPlayer = new PlayerController(i);
				aiPlayers.Add(aiPlayer);
			}
		} else {
			Debug.Print("Invalid player count");
		}
	}

	/* ==================== AI Controls ==================== */
	
	private void nextTurn()
	{
		Debug.Print("Ending player turn");
		aiThinking = true;
		currentPlayer = 1;
		startAITurn();
	}

	private void startAITurn()
	{
		aiThread = new Thread(RunAI);
		aiThread.Start();
	}

	private void RunAI()
	{
		// TODO: Do AI decision making for currentPlayer
		Debug.Print($"Running AI decision making for player {currentPlayer}");

		CallDeferred(MethodName.OnAIFinish);
	}

	private void OnAIFinish()
	{
		aiThread.Join();

		// TODO: Apply AI decisions for currentPlayer
		Debug.Print($"Applying AI decisions for player {currentPlayer}");

		currentPlayer = (currentPlayer + 1) % playerCount;

		if (currentPlayer == 0)
		{
			// All AI players have gone, return control to user
			aiThinking = false;
			Debug.Print("Returning control to user player");
		}
		else
		{
			// Pass turn to next AI player
			startAITurn();
		}
	}

}
