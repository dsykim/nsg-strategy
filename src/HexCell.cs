using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class HexCell : Sprite2D
{
	public readonly int x, y;
	public TerrainTypes terrainType;
	private List<CellDecorator> decorators = new List<CellDecorator>();
	
	public HexCell(int x, int y) {
		this.x = x;
		this.y = y;
		terrainType = TerrainTypes.EMPTY;
	}
	
	public HexCell(int x, int y, TerrainTypes tType) : this(x, y) {
		terrainType = tType;
		switch (terrainType)
		{
			case TerrainTypes.OCEAN:
				break;
			case TerrainTypes.HILLS:
				
			
			case TerrainTypes.PLAINS:
				
			case TerrainTypes.MOUNTAIN:
				
			case TerrainTypes.EMPTY:
				break;
		}
	}

	public bool isDecoratorAllowed(CellDecorator dec) {
		return true;
	}
	public List<CellDecorator> getDecorators() {
		return decorators;
	}
	
	public void addDecorator(CellDecorator dec) {
		if (isDecoratorAllowed(dec)) {
			decorators.Add(dec);
			AddChild(dec);
		}
	}
}

