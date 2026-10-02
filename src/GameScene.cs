using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GameScene : Node
{
	public static GameScene instance { get; private set; }
	
	private TurnController turnController;
	private CameraController camera;
	private UIController uiController;
	
	public GameState state { get; private set; }

	private readonly GameConfig config = new GameConfig
	{
			playerCount = 4,
			width = 41,
			height = 24,
			seed = 1
	};

	public override void _Ready() {
		state = GameSetup.setupGame(config);
		CommandExecutor commandExecutor = new CommandExecutor(state);
		
		turnController = new TurnController();
		AddChild(turnController);
		turnController.init(state, commandExecutor, [1]);

		uiController = (UIController)GetNode("UIController");
		uiController.init();

		camera = (CameraController)GetNode("MainCamera");
		camera.init();
		
		InputController input = new InputController();
		input.init();
		AddChild(input);
		
		turnController.startGame();
	}

	public override void _Process(double delta) {

	}
}
