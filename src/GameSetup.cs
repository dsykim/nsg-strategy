
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public struct GameConfig
{
	public int playerCount, width, height, seed;
}


public static class GameSetup
{
	public static GameState setupGame(GameConfig config) {
		GameState state = new GameState();
		state.rng = new Random(config.seed);
		UnitDefs.load();
		state.grid = mirror(generateMap(config.width, config.height, config.seed));
		for (int i = 1; i < config.playerCount + 1; i++) {
			PlayerState player = new PlayerState();
			player.playerID = i;
			player.unitCapacityTotal = 10;
			player.gold = 20;
			UnitRules.placeUnit(state, i, UnitType.SETTLER, getSpawnPos(state, i));
			state.players.Add(i, player);
		}

		return state;
	}

	private static Vector2I getSpawnPos(GameState state, int playerID) {
		List<HexCell> validSpawns = state.grid.grid.Where(c => MapRules.isPassable(state, c.pos)).ToList();
		if (playerID == 1) return validSpawns[0].pos;
		if (playerID == 2) return validSpawns[validSpawns.Count-1].pos;
		return new Vector2I();
	}

	// private static HexGrid generateMap(int width, int height, int seed) {
	// 	HexGrid grid = new HexGrid(width, height);
	// 	foreach (var cell in grid.grid) {
	// 		grid.setCell(cell.pos, TerrainType.PLAINS);
	// 	}
	//
	// 	return grid;
	// }

	private static HexGrid generateMap(int width, int height, int seed) {
		Queue<HexCell> frontier = new Queue<HexCell>();
		HexGrid grid = new HexGrid(width, height);
		Random rand = new Random(seed);
	
		Vector2I startPos = new Vector2I(width / 2, height / 2);
		grid.setCell(startPos, TerrainType.PLAINS);
		
		foreach (HexCell c in grid.getNeighbors(startPos)) {
			frontier.Enqueue(c);
		}
	
		while (frontier.Count > 0) {
			HexCell next = frontier.Dequeue();
			bool isBorder = next.pos.X == 0 ||
							next.pos.X == width - 1 ||
							next.pos.Y == 0 ||
							next.pos.Y == height - 1;
			bool isBorderAdj = next.pos.X == 1 ||
							   next.pos.X == width - 2 ||
							   next.pos.Y == 1 ||
							   next.pos.Y == height - 2;
			int distToStart = HexUtils.hexDistance(next.pos, startPos);
			float threshold = Math.Max(0.95f - (float)Math.Pow((float)distToStart / width, 2), 0.25f);
	
			if (isBorderAdj) threshold = 0.2f;
	
			if (!isBorder && rand.NextSingle() < threshold) {
				// Make land
				float randVal = rand.NextSingle();
				TerrainType lType;
				if (randVal < 0.6) {
					lType = TerrainType.PLAINS;
				} else if (randVal < 0.9) {
					lType = TerrainType.HILLS;
				} else {
					lType = TerrainType.MOUNTAIN;
				}
	
				grid.setCell(next.pos, lType);
				foreach (HexCell c in grid.getNeighbors(next.pos)) {
					if (!frontier.Contains(c) && c.terrainType == TerrainType.EMPTY) {
						frontier.Enqueue(c);
					}
				}
			} else {
				// Make ocean
				grid.setCell(next.pos, TerrainType.OCEAN);
			}
		}
	
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Vector2I pos = new Vector2I(x, y);
				if (grid.getCell(pos).terrainType == TerrainType.EMPTY) {
					grid.setCell(pos, TerrainType.OCEAN);
				}
			}
		}
	
		return grid;
	}
	
	public static bool isBorder(HexGrid grid, int x, int y) {
		return x == 0 || x == grid.width - 1 || y == 0 || y == grid.height - 1;
	}

	public static HexGrid mirror(HexGrid grid) {
		int halfWidth = (grid.width % 2 == 0) ? grid.width / 2 - 1 : grid.width / 2;
		for (int x = grid.width - 1; x > halfWidth; x--) {
			for (int y = 0; y < grid.height; y++) {
				Vector2I pos = new Vector2I(x, y);
				Vector2I mirroredPos = new Vector2I(grid.width - x - 1, y);
				grid.setCell(pos, grid.getCell(mirroredPos).terrainType);
			}
		}
		return grid;
	}
	
}
