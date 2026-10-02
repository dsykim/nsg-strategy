using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MapRules
{
	public static readonly HashSet<TerrainTypes> impassable =
			new() { TerrainTypes.EMPTY, TerrainTypes.OCEAN, TerrainTypes.MOUNTAIN };
	
	public static bool isPassable(GameState s, Vector2I pos) =>
			!impassable.Contains(s.grid.getCell(pos).terrainType);

	public static bool canPlaceUnit(GameState s, Vector2I pos) =>
			isPassable(s, pos) && !s.grid.getCell(pos).hasUnit();

	public static bool canPlaceCity(GameState s, Vector2I pos) {
		HexCell cell = s.grid.getCell(pos);
		if (cell.hasController() || !isPassable(s, pos)) return false;
		foreach (Vector2I p in s.grid.getCellsPosInRadius(pos, 2))
			if (s.grid.getCell(p).hasCity(s)) return false;
		return true;
	}

	public static Dictionary<Vector2I, int> movableCells(GameState s, Unit unit) {
		var reachable = new Dictionary<Vector2I, int>();
		var visited = new HashSet<Vector2I> { unit.gridPosition };
		var frontier = new Queue<(Vector2I pos, int cost)>();
		frontier.Enqueue((unit.gridPosition, 0));

		while (frontier.Count > 0) {
			var (pos, cost) = frontier.Dequeue();
			if (cost >= unit.currentAP) continue;

			foreach (Vector2I n in s.grid.getNeighborPositions(pos)) {
				if (!visited.Add(n) || !isPassable(s, n)) continue;

				HexCell cell = s.grid.getCell(n);
				bool canStop = !cell.hasUnit();
				bool canTraverse = canStop || s.getUnit(cell.unitID).owner == unit.owner;

				if (canStop) reachable.Add(n, cost + 1);
				if (canTraverse) frontier.Enqueue((n, cost + 1));
			}
		}
		return reachable;
	}

	public static bool canMove(GameState s, Unit unit, Vector2I target) =>
			movableCells(s, unit).ContainsKey(target);

	public static (Unit, City) getCellDefenders(GameState s, Vector2I pos) {
		HexCell cell = s.grid.getCell(pos);
		(Unit, City) defenders = (null, null);
		if (cell.hasUnit()) defenders.Item1 = s.getUnit(cell.unitID);
		if (cell.hasCity(s)) defenders.Item2 = s.getCity(cell.playerDecoratorID);
		return defenders;

	}
	
	public static int pathDistance(GameState s, Vector2I start, Vector2I goal) {
		if (start == goal) return 0;

		var visited = new HashSet<Vector2I> { start };
		var frontier = new Queue<(Vector2I pos, int cost)>();
		frontier.Enqueue((start, 0));

		while (frontier.Count > 0) {
			var (pos, cost) = frontier.Dequeue();

			foreach (HexCell neighbor in s.grid.getNeighbors(pos)) {
				if (!visited.Add(neighbor.pos)) continue; // first reach = shortest (unit weights)
				if (impassable.Contains(neighbor.terrainType)) continue;

				if (neighbor.pos == goal) return cost + 1;
				frontier.Enqueue((neighbor.pos, cost + 1));
			}
		}

		return -1; // no walkable path exists
	}

	public static void moveUnit(GameState s, Unit unit, Vector2I target) {
		var costs = movableCells(s, unit);
		int cost = costs[target];
		s.grid.getCell(unit.gridPosition).clearUnit();
		unit.gridPosition = target;
		s.grid.getCell(target).setUnit(unit);
		unit.spendAP(cost);
	}
}