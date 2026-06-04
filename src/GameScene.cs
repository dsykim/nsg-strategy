using Godot;
using System;
using System.Diagnostics;
		
public partial class GameScene : Node
{
	private MapController mapController;
	private readonly int width = 16;
	private readonly int height = 8;
	private readonly float hexSize = 40f;
	
	public override void _Ready() {
		HexGrid hexGrid = new HexGrid(width, height);
		AddChild(hexGrid);

		mapController = new MapController(hexGrid, hexSize);
		mapController.generateMap();
	}

	public override void _Process(double delta) {
		
	}
}
