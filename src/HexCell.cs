using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class HexCell : Sprite2D
{
	public readonly int x, y;
	public TerrainTypes terrainType;
	public NaturalDecorator naturalDecorator = null;
	public PlayerDecorator playerDecorator = null;
	public City city = null;
	public List<Unit> units = new List<Unit>();
	
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

	public bool hasCity() {
		return city != null;
	}

	public bool hasUnit() {
		return units.Count > 0;
	}

	public bool hasNaturalDecorator() {
		return naturalDecorator != null;
	}

	public bool hasPlayerDecorator() {
		return playerDecorator != null;
	}
}

