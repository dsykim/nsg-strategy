using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GameScene : Node
{
	public static GameScene instance { get; private set; }
	
	private TurnController turnController;
	private BoardView boardView;
	private CameraController camera;
	// private UIController uiController;
	
	public GameState state { get; private set; }

	private readonly GameConfig config = new GameConfig
	{
			playerCount = 2,
			width = 21,
			height = 16,
			seed = 1
	};

	public override void _Ready() {
		state = GameSetup.setupGame(config);
		CommandExecutor commandExecutor = new CommandExecutor(state);
		
		if (DisplayServer.GetName() != "headless") {
			boardView = new BoardView();
			AddChild(boardView);
			boardView.init(state, commandExecutor);
			
			camera = new CameraController();
			AddChild(camera);
			camera.init(boardView.boardSize);
		}

		
		turnController = new TurnController();
		AddChild(turnController);
		turnController.init(state, commandExecutor, []);

		// uiController = (UIController)GetNode("UIController");
		// uiController.init();
		
		//
		// InputController input = new InputController();
		// input.init();
		// AddChild(input);
		
		turnController.startGame();
	}

	public override void _Process(double delta) {

	}
}
