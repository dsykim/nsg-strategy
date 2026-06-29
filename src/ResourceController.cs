using Godot;
using System;
using Godot.Collections;
using System.Diagnostics;

public partial class ResourceController : Node
{
	[Signal]
	public delegate void ResourceUpdatedEventHandler(Dictionary<string, int> vals);

	public int gold { get; private set; }
	public int goldRate { get; private set; }

	public int unitCapacityTotal { get; private set; }
	public int unitCapacityUsed { get; private set; }

	private int id;

	public ResourceController(int id) {
		gold = 20;
		goldRate = 0;
		this.id = id;
		Name = "ResourceController";
		emitResourceUpdated();
	}

	public void addGold(int delta) {
		gold += delta;
		emitResourceUpdated();
	}

	public void addUnitCapacityUsed(int delta) {
		unitCapacityUsed += delta;
		emitResourceUpdated();
	}
	
	public void addUnitCapacityTotal(int delta) {
		unitCapacityTotal += delta;
		emitResourceUpdated();
	}

	public bool canAfford(int cost) {
		return cost <= gold;
	}
	public void resourceUpkeep() {
		gold = Math.Max(0, gold + goldRate);
		emitResourceUpdated();
	}

	public void handleUnitCreatedSignal(Unit unit) {
		gold -= unit.goldCost;
		emitResourceUpdated();
	}

	public void handleCityCreatedSignal(City city) {
		goldRate += city.goldProduction;
		emitResourceUpdated();
	}
	
	public bool hasCapacityForUnit(Unit unit) {
		// TODO: move constants to JSON so we can access unit values without needing to create the object.
		return unit.capacityCost <= unitCapacityTotal - unitCapacityUsed;
	}

	public bool hasCapacityForUnit(int capacityCost) {
		return capacityCost <= unitCapacityTotal - unitCapacityUsed;
	}

	private void emitResourceUpdated() {
		Dictionary<string, int> vals = new Dictionary<string, int>();
		vals["gold"] = gold;
		vals["unitCapacityTotal"] = unitCapacityTotal;
		vals["unitCapacityUsed"] = unitCapacityUsed;
		EmitSignal(SignalName.ResourceUpdated, vals);
	}
}
