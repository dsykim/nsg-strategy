using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum TerrainType
{
	PLAINS,
	HILLS,
	MOUNTAIN,
	OCEAN,
	EMPTY
}

public class HexCell
{
	public readonly Vector2I pos;
	public int controllerID = 0;
	public TerrainType terrainType;

	public int naturalDecoratorID = 0;
	public int playerDecoratorID = 0;
	public int unitID = 0;

	public HexCell(Vector2I pos) {
		this.pos = pos;
		terrainType = TerrainType.EMPTY;
	}

	public HexCell clone() {
		return (HexCell)MemberwiseClone();
	}
	
	public HexCell(Vector2I pos, TerrainType tType) : this(pos) {
		terrainType = tType;
	}

	public bool hasCity(GameState state) {
		return state.getEntity(playerDecoratorID) is City;
	}

	public void clearPlayerDecorator() => playerDecoratorID = 0;

	public void setPlayerDecorator(PlayerDecorator d) => playerDecoratorID = d.id;

	public bool hasUnit() {
		return unitID != 0;
	}

	public void clearUnit() => unitID = 0;

	public void setUnit(Unit u) => unitID = u.id;
	

	public bool hasController() {
		return controllerID > 0;
	}

	public void setController(int id) => controllerID = id;

	public void clearController() => controllerID = 0;
}
