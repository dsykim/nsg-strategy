using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerState
{
	public int playerID, gold, goldRate, unitCapacityTotal, unitCapacityUsed;
	public bool alive = true;

	public PlayerState clone() => (PlayerState)MemberwiseClone();

	public override string ToString() {
		return $"""
		       Player {playerID}; Alive {alive}; Gold {gold}; Gold Rate {goldRate}; Unit Capacity {unitCapacityUsed}/{unitCapacityTotal}  
		       """;
	}

}

public class GameState
{
	public HexGrid grid;
	public readonly Dictionary<int, CellDecorator> entities = new();
	public readonly Dictionary<int, PlayerState> players = new();
	public int nextId = 1; // 0 reserved as "unassigned"
	public int currentPlayer = 1;
	public int turnNumber;
	public Random rng = new();

	public int register(CellDecorator e) {
		int id = nextId++;
		e.assignId(id);
		entities[id] = e;
		return id;
	}

	public void registerExisting(CellDecorator e, int id) {
		e.assignId(id);
		entities[id] = e;
		if (id >= nextId) nextId = id + 1;
	}

	public void unregister(int id) => entities.Remove(id);

	public CellDecorator getEntity(int id) => entities.TryGetValue(id, out var e) ? e : null;
	public Unit getUnit(int id) => getEntity(id) as Unit;
	public City getCity(int id) => getEntity(id) as City;
	public IEnumerable<Unit> allUnits() => entities.Values.OfType<Unit>();
	public IEnumerable<City> allCities() => entities.Values.OfType<City>();
	public PlayerState getPlayer(int id) => players.TryGetValue(id, out var p) ? p : null;

	public GameState clone() {
		var c = new GameState
		{
				grid = grid.clone(),
				nextId = nextId,
				rng = new Random(rng.Next()),
				currentPlayer = currentPlayer,
				turnNumber = turnNumber,
		};
		foreach (var (id, e) in entities) c.entities[id] = e.clone();
		foreach (var (id, p) in players) c.players[id] = p.clone();
		return c;
	}

	public override string ToString() {
		string s = "";
		foreach (var player in players) {
			s += player.Value.ToString() + "\n";
		}

		foreach (var entity in entities) {
			Type eType = entity.Value.GetType();
			string eString = $"ID {entity.Key}; {eType.ToString()}; Pos {entity.Value.gridPosition}";
			if (entity.Value is PlayerDecorator) {
				PlayerDecorator d = (PlayerDecorator)entity.Value;
				eString += $"; Owner {d.owner}";
			}
			if (entity.Value is Unit) {
				Unit u = (Unit)entity.Value;
				eString += $"; HP {u.currentHP}/{u.maxHP}";
			} else if (entity.Value is City) {
				City c = (City)entity.Value;
				eString += $"; HP {c.currentHP}/{c.maxHP}";
			}
			
			s += eString + "\n";
		}
		return s;
	}
}
