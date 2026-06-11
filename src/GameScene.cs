using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GameScene : Node
{
	private TurnController turnController;
	private CameraController camera;
	private UIController uiController;

	private readonly int width = 20;
	private readonly int height = 12;
	private readonly float hexSize = 40f;

	public override void _Ready() {
		MapController mapController = new MapController(width, height, hexSize);
		AddChild(mapController);
		mapController.generateMap();

		InputController input = new InputController();
		input.init();
		AddChild(input);

		turnController = new TurnController();
		AddChild(turnController);
		turnController.init(4);

		uiController = (UIController)GetNode("UIController");
		uiController.init();

		camera = (CameraController)GetNode("MainCamera");
		camera.init();
	}

	public override void _Process(double delta) {

	}
}
