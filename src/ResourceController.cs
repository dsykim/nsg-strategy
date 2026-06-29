using Godot;
using System;
using Godot.Collections;
using System.Diagnostics;

public partial class ResourceController : Node
{
	[Signal]
	public delegate void ResourceUpdatedEventHandler(Dictionary<string, int> vals);

	public int gold { get; private set; }
	public int goldRate;

	private int id;

	public ResourceController(int id) {
		gold = 20;
		goldRate = 0;
		this.id = id;
		Name = "ResourceController";
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

	private void emitResourceUpdated() {
		Dictionary<string, int> vals = new Dictionary<string, int>();
		vals["gold"] = gold;
		EmitSignal(SignalName.ResourceUpdated, vals);
	}
}
