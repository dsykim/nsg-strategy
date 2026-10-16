
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public struct GameConfig
{
	public int playerCount, boardSize, seed;
}

public static class GameSetup
{
	private static int MAX_PLAYERS = 2;
	public static GameState setupGame(GameConfig config) {
		GameState state = new GameState();
		state.rng = new Random(config.seed);
		UnitDefs.load();
		state.grid = mirror(generateMap(config));
		for (int i = 1; i < config.playerCount + 1; i++) {
			PlayerState player = new PlayerState();
			player.playerID = i;
			player.unitCapacityTotal = 10;
			player.gold = 20;
			UnitRules.placeUnit(state, i, UnitType.SETTLER, getSpawnPos(config, i));
			state.players.Add(i, player);
		}

		state.updateVisibility();
		return state;
	}

	private static Vector2I getSpawnPos(GameConfig config, int playerID) {
		if (playerID > MAX_PLAYERS) GD.PrintErr($"Too many players. Max player count is {MAX_PLAYERS}");
		if (playerID == 1) return new Vector2I(3, config.boardSize);
		if (playerID == 2) return new Vector2I(2 * config.boardSize - 3, config.boardSize);
		return new Vector2I();
	}

	private static HexGrid generateMap(GameConfig config) {
		int width = config.boardSize * 2 + 1;
		int height = width;
		HexGrid grid = new HexGrid(width, height);
		Random rand = new Random(config.seed);
		int radius = (width - 2) / 2;
		Vector2I center = new Vector2I(width, height) / 2;
		
		foreach (HexCell c in grid.grid) {
			if (grid.getCellsInRadius(center, radius).Contains(c)) {
				// Tile on board
				float val = rand.NextSingle();
				TerrainType tType;
				if (val < 0.4f) {
					tType = TerrainType.PLAINS;
				} else if (val < 0.7f) {
					tType = TerrainType.HILLS;
				} else if (val < 0.85f) {
					tType = TerrainType.MOUNTAIN;
				} else {
					tType = TerrainType.OCEAN;
				}
				grid.setCell(c.pos, tType);
			} else if (grid.getCellsInRadius(center, radius+1).Contains(c)){
				// Tile not on board, ocean
				grid.setCell(c.pos, TerrainType.OCEAN);
			} else {
				grid.setCell(c.pos, TerrainType.EMPTY);
			}
		}

		for (int i = 1; i < MAX_PLAYERS + 1; i++) {
			Vector2I spawnPos = getSpawnPos(config, i);
			grid.setCell(spawnPos, TerrainType.PLAINS);
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
