
using Godot;
using System;
using System.Collections.Generic;

public struct GameConfig
{
	public int playerCount, width, height, seed;
}


public static class GameSetup
{
	public static GameState setupGame(GameConfig config) {
		GameState state = new GameState();
		UnitDefs.load();
		state.grid = mirror(generateMap(config.width, config.height, config.seed));
		for (int i = 0; i < config.playerCount; i++) {
			PlayerState player = new PlayerState();
			player.playerID = i + 1;
			player.unitCapacityTotal = 10;
			player.gold = 20;
			state.players.Add(i + 1, player);
		}

		return state;
	}

	private static HexGrid generateMap(int width, int height, int seed) {
		Queue<HexCell> frontier = new Queue<HexCell>();
		HexGrid grid = new HexGrid(width, height);
		Random rand = new Random(seed);

		Vector2I startPos = new Vector2I(width / 2, height / 2);
		grid.setCell(startPos, TerrainTypes.PLAINS);
		
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
				TerrainTypes lType;
				if (randVal < 0.6) {
					lType = TerrainTypes.PLAINS;
				} else if (randVal < 0.9) {
					lType = TerrainTypes.HILLS;
				} else {
					lType = TerrainTypes.MOUNTAIN;
				}
	
				grid.setCell(next.pos, lType);
				foreach (HexCell c in grid.getNeighbors(next.pos)) {
					if (!frontier.Contains(c) && c.terrainType == TerrainTypes.EMPTY) {
						frontier.Enqueue(c);
					}
				}
			} else {
				// Make ocean
				grid.setCell(next.pos, TerrainTypes.OCEAN);
			}
		}
	
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Vector2I pos = new Vector2I(x, y);
				if (grid.getCell(pos).terrainType == TerrainTypes.EMPTY) {
					grid.setCell(pos, TerrainTypes.OCEAN);
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
