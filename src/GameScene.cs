using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
		
public partial class GameScene : Node
{
	private TurnController turnController;
	
	private readonly int width = 20;
	private readonly int height = 12;
	private readonly float hexSize = 40f;
	
	public override void _Ready() {
		MapController mapController = new MapController(width, height, hexSize);
		AddChild(mapController);
		mapController.generateMap();

		turnController = new TurnController();
		AddChild(turnController);
		turnController.init(4);
	}

	public override void _Process(double delta) {
		
	}
}
