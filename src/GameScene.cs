using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
		
public partial class GameScene : Node
{
	private PlayerController userPlayer;
	private List<PlayerController> aiPlayers;
	
	private readonly int width = 42;
	private readonly int height = 24;
	private readonly float hexSize = 20f;
	
	public override void _Ready() {
		MapController mapController = new MapController(width, height, hexSize);
		AddChild(mapController);
		mapController.generateMap();
	}

	public override void _Process(double delta) {
		
	}
}
