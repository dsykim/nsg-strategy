using Godot;
using System.Collections.Generic;

public partial class EntityRegistry : Node
{
	public static EntityRegistry instance { get; private set; }

	private readonly Dictionary<int, CellDecorator> entities = new();
	private int nextId = 1;   // 0 reserved as "unassigned"

	public EntityRegistry() {
		instance = this;
		Name = "EntityRegistry";
	}

	// New entity created during play: allocate a fresh id.
	public int register(CellDecorator e) {
		int id = nextId++;
		e.assignId(id);
		entities[id] = e;
		return id;
	}

	// Entity reconstructed from a save: reuse its persisted id.
	public void registerExisting(CellDecorator e, int id) {
		e.assignId(id);
		entities[id] = e;
		if (id >= nextId) nextId = id + 1;   // self-heal: never reissue a loaded id
	}

	public void unregister(int id) => entities.Remove(id);

	public CellDecorator getEntity(int id) =>
			entities.TryGetValue(id, out var e) ? e : null;

	public Unit getUnit(int id) => getEntity(id) as Unit;
	public City getCity(int id) => getEntity(id) as City;

	// Save/load hooks for the counter.
	public int getNextId() => nextId;
	public void setNextId(int n) => nextId = n;
}
