using Godot;
using System;
using System.Diagnostics;
		
public partial class GameScene : Node
{
	private HexGrid hexGrid;
	private readonly int width = 10;
	private readonly int height = 10;
	
	public override void _Ready() {
		hexGrid = new HexGrid(width, height);
	}

	public override void _Process(double delta) {
		
	}
}
