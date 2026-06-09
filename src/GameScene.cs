using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
		
public partial class GameScene : Node
{
	private PlayerController userPlayer;
	private List<PlayerController> aiPlayers;
	
	private readonly int width = 20;
	private readonly int height = 12;
	private readonly float hexSize = 40f;
	
	public override void _Ready() {
		MapController mapController = new MapController(width, height, hexSize);
		AddChild(mapController);
		mapController.generateMap();

		MeleeUnit unit1 = new MeleeUnit();
		mapController.addUnit(unit1, new Vector2I(width/2, height/2));

		City city1 = new City();
		mapController.addCity(city1, new Vector2I(width/2 + 1, height/2 + 1));
	}

	public override void _Process(double delta) {
		
	}
}
