using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;

public static class CityRules
{
	public static void createCity(GameState s, int playerID, Vector2I pos) {
		if (!MapRules.canPlaceCity(s, pos)) {
			Debug.Print("Cannot place city at " + pos);
			return;
		}
		City city;
		if (s.playerCities(playerID).Count == 0) {
			city = new Capital(playerID, pos);
			s.getPlayer(playerID).hasSettled = true;
		} else {
			city = new City(playerID, pos);
		}
		
		s.register(city);
		s.grid.getCell(pos).setPlayerDecorator(city);
		s.getPlayer(playerID).goldRate += city.goldProduction;
		claimCell(s, city, 2); // TODO: Change default settle radius to parameter in playerState
	}

	public static void deleteCity(GameState s, City c) {
		s.grid.getCell(c.pos).clearPlayerDecorator();
		s.getPlayer(c.owner).goldRate -= c.goldProduction;
		foreach (var cellPos in c.ownedCells) {
			s.grid.getCell(cellPos).clearController();
		}
		s.unregister(c.id);
	}

	public static void claimCell(GameState s, City c, int radius) {
		var inRangeCells = s.grid.getCellsInRadius(c.pos, radius);
		foreach (var cell in inRangeCells) {
			if (!cell.hasController()) {
				c.ownedCells.Add(cell.pos);
				cell.setController(c.owner);
			}
		}
	}

	public static IEnumerable<Vector2I> getVisibleCells(GameState s, City c) {
		return s.grid.getCellsPosInRadius(c.pos, 3);
	}
}
