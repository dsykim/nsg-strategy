using Godot;
using System;

public partial class HexCell : Sprite2D
{
	public int x, y;
	public TerrainTypes terrainType;
	
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
}

