using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerState
{
	public int playerID, gold, goldRate, unitCapacityTotal, unitCapacityUsed;
	public bool alive = true;
	public List<Vector2I> visibleCells = new List<Vector2I>();

	public PlayerState clone() {
		PlayerState clone = (PlayerState)MemberwiseClone();
		clone.visibleCells = new List<Vector2I>(visibleCells);
		return clone;
	}

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
	public bool fogOfWar = false;

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

	public List<Unit> playerUnits(int playerID) => allUnits().Where(u => u.owner == playerID).ToList();
	public List<City> playerCities(int playerID) => allCities().Where(u => u.owner == playerID).ToList();

	public void updateVisibility() {
		foreach (var p in players) {
			p.Value.visibleCells = MapRules.playerVisibleCells(this, p.Key);
		}
	}
	
	public GameState clone() {
		var c = new GameState
		{
				grid = grid.clone(),
				nextId = nextId,
				rng = new Random(rng.Next()),
				currentPlayer = currentPlayer,
				turnNumber = turnNumber,
				fogOfWar = fogOfWar,
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

		foreach (var (id, entity) in entities) {
			string eString = $"ID {id}; {entity.GetType()}; Pos {entity.pos}";
			if (entity is PlayerDecorator d) eString += $"; Owner {d.owner}";
			if (entity is IHasHealth h) eString += $"; HP {h.currentHP}/{h.maxHP}";
			s += eString + "\n";
		}
		return s;
	}
}
