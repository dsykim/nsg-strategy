using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum UnitType
{
	SETTLER,
	MELEE,
	RANGED,
	SNIPER,
	SCOUT
}

public static class UnitRules
{
	public static IEnumerable<Unit> unitsOf(GameState s, int playerID) =>
			s.allUnits().Where(u => u.owner == playerID);

	public static bool canCreate(GameState s, int playerID, UnitType type, Vector2I pos) {
		PlayerState p = s.getPlayer(playerID);
		UnitDef def = UnitDefs.get(type);
		return p != null &&
			   p.gold >= def.goldCost &&
			   p.unitCapacityTotal - p.unitCapacityUsed >= def.capacityCost &&
			   MapRules.canPlaceUnit(s, pos);
	}

	public static Unit createUnit(GameState s, int playerID, UnitType type, Vector2I pos) {
		Unit u = placeUnit(s, playerID, type, pos);
		PlayerState p = s.getPlayer(playerID);
		p.gold -= u.goldCost;
		p.unitCapacityUsed += u.capacityCost;
		return u;
	}

	public static Unit placeUnit(GameState s, int playerID, UnitType type, Vector2I pos) {
		Unit u = type switch
		{
				UnitType.SETTLER => new SettlerUnit(playerID),
				UnitType.MELEE => new MeleeUnit(playerID),
				UnitType.RANGED => new RangedUnit(playerID),
				UnitType.SNIPER => new SniperUnit(playerID),
				UnitType.SCOUT => new ScoutUnit(playerID),
				_ => throw new ArgumentException($"Unknown unit type {type}"),
		};
		u.pos = pos;
		s.register(u);
		s.grid.getCell(pos).setUnit(u);
		
		return u;
	}
	
	public static void deleteUnit(GameState s, Unit u) {
		s.grid.getCell(u.pos).clearUnit();
		s.getPlayer(u.owner).unitCapacityUsed -= u.capacityCost;
		s.unregister(u.id);
	}

	public static void upkeep(GameState s, int playerID) {
		foreach (Unit u in unitsOf(s, playerID)) u.restoreAP();
	}

	public static List<Vector2I> getAttackableCells(GameState s, Unit u) {
		List<Vector2I> attackable = new List<Vector2I>();
		int attackerID = u.owner;
		foreach (var pos in s.grid.getCellsInRadiusRange(u.pos, u.minRange, u.maxRange)) {
			HexCell defenderCell = s.grid.getCell(pos);
			bool hasEnemyCity = defenderCell.hasCity(s) && defenderCell.controllerID != attackerID;
			bool hasEnemyUnit = defenderCell.hasUnit() && s.getUnit(defenderCell.unitID).owner != attackerID;
			if ((hasEnemyCity || hasEnemyUnit) && HexUtils.inLineOfSight(s, u.pos, pos)) {
				attackable.Add(pos);
			}
		}
		
		return attackable;
	}

	public static List<Vector2I> getCellsInAttackRange(GameState s, Unit u) {
		return s.grid.getCellsInRadiusRange(u.pos, u.minRange, u.maxRange)
				.Where(pos => HexUtils.inLineOfSight(s, u.pos, pos) 
							  && MapRules.isPassable(s, pos)).ToList();
	}

	public static List<Vector2I> getVisibleCells(GameState s, Unit u) {
		int viewRange = (s.grid.getCell(u.pos).terrainType == TerrainType.HILLS) ? u.viewRange + 1 : u.viewRange;
		var cellsInRange = s.grid.getCellsPosInRadius(u.pos, viewRange);
		return cellsInRange.Where(c => HexUtils.inLineOfSight(s, u.pos, c) || 
									   s.grid.getCell(c).terrainType == TerrainType.MOUNTAIN).ToList();
	}
}
