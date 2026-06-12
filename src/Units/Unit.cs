using Godot;
using System.Collections.Generic;
using System.Text.Json.Nodes;

public static class UnitDataLoader
{
	private static JsonObject _data;

	public static JsonObject GetUnitData(string unitKey)
	{
		if (_data == null)
		{
			string json = FileAccess.GetFileAsString("res://src/Units/Units.json");
			_data = JsonNode.Parse(json)!.AsObject();
		}

		return _data[unitKey]!.AsObject();
	}
}

public abstract partial class Unit : CellDecorator
{
	public int maxHP;
	public int currentHP;
	public int maxAP;
	public int currentAP;

	public int goldCost;
	public int capacityCost;

	/** ID of the player who owns this unit. */
	public readonly int owner;

	/** A list of this unit's actions. */
	public List<UnitAction> actions { get; private set; } = new List<UnitAction>();

	/** Signal triggered when action availability is updated. */
	[Signal] public delegate void actionsChangedEventHandler();

	public Unit(int owner)
	{
		this.owner = owner;
		ZIndex = 9;
	}
	
	protected void LoadFromData(string unitKey)
	{
		JsonObject data = UnitDataLoader.GetUnitData(unitKey);

		maxHP        = data["maxHP"]!.GetValue<int>();
		maxAP        = data["maxAP"]!.GetValue<int>();
		currentAP    = maxAP;
		capacityCost = data["capacityCost"]!.GetValue<int>();
		goldCost = data["goldCost"]!.GetValue<int>();
		Texture      = ResourceLoader.Load<Texture2D>(data["texture"]!.GetValue<string>());
	}

	public void setCurrentAP(int val)
	{
		currentAP = val;
	}

	public void addAction(UnitAction action)
	{
		actions.Add(action);
	}

	/** Updates the availability of this unit's actions. */
	public void updateAvailability(string id, bool available)
	{
		UnitAction action = actions.Find(a => a.id == id);
		if (action != null)
		{
			action.isAvailable = available;
			EmitSignal(SignalName.actionsChanged);
		}
	}
}
