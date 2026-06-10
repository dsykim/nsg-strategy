using Godot;
using System;
using Godot.Collections;
using System.Diagnostics;

public partial class ResourceController : Node
{
	[Signal]
	public delegate void ResourceUpdatedEventHandler(Dictionary<string, int> vals);

	public int stone { get; private set; }
	public int stoneRate;

	private int id;

	public ResourceController(int id) {
		stone = 0;
		stoneRate = 1;
		this.id = id;
		Name = "ResourceController";
	}

	public void resourceUpkeep() {
		stone = Math.Max(0, stone + stoneRate);
		Dictionary<string, int> vals = new Dictionary<string, int>();
		vals["stone"] = stone;
		EmitSignal(SignalName.ResourceUpdated, vals);
	}
}
