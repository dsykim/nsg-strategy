using Godot;
using System;

/**
 * Composition root: builds the model, then the view/input/UI layers, and hands each
 * one its dependencies through init. Nothing is reached through static instances.
 */
public partial class GameScene : Node
{
	/** HUD root from the scene file; optional (missing in headless or bare test scenes). */
	[Export] private UIController uiController;

	/** Players controlled by a human. Ignored in headless runs (AI vs AI). */
	private readonly int[] humanPlayerIDs = { 1, 2 };

	private readonly GameConfig config = new GameConfig
	{
			playerCount = 2,
			boardSize = 9,
			seed = 1
	};

	private GameState state;
	private TurnController turnController;

	public override void _Ready() {
		state = GameSetup.setupGame(config);
		var executor = new CommandExecutor(state);
		bool headless = DisplayServer.GetName() == "headless";

		turnController = new TurnController();
		AddChild(turnController);
		turnController.init(state, executor, headless ? Array.Empty<int>() : humanPlayerIDs);

		if (!headless) buildPresentation(executor);

		turnController.startGame();
	}

	private void buildPresentation(CommandExecutor executor) {
		// Selection first: it must revalidate before any other StateChanged listener reads it.
		var selection = new Selection(state, executor);

		var boardView = new BoardView();
		AddChild(boardView);
		boardView.init(state, executor);

		var camera = new CameraController();
		AddChild(camera);
		camera.init(boardView.boardSize);

		var input = new InputController();
		AddChild(input);
		input.init(state, executor, boardView, selection);

		uiController?.init(new HudContext(state, executor, selection, input));
	}
}
