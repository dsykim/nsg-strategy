using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

public class UnitAction
{
	public string id;
	public string label;
	public string keyBinding;
	public bool isAvailable;
	public Action onTrigger;
}


public abstract partial class Unit : CellDecorator
{
	public int maxHP;
	public int currentHP;
	public int maxAP;
	public int currentAP;

	public int range;
	public int damage;
	public int attackCost;

	public int goldCost;
	public int capacityCost;

	/** ID of the player who owns this unit. */
	public readonly int owner;

	public string unitName;

	/** A list of this unit's actions. */
	public List<UnitAction> actions { get; private set; } = new List<UnitAction>();

	/** Signal triggered when action availability is updated. */
	[Signal] public delegate void actionsChangedEventHandler();

	[Signal]
	public delegate void statsChangedEventHandler();


	public Unit(int owner)
	{
		this.owner = owner;
		ZIndex = 9;
	}
	
	protected void LoadFromData(string unitKey)
	{
		string json = FileAccess.GetFileAsString("res://src/Units/Units.json");
		var _data = JsonNode.Parse(json)!.AsObject();
		JsonObject data = _data[unitKey]!.AsObject();

		unitName = data["name"]!.GetValue<string>();
		maxHP = data["maxHP"]!.GetValue<int>();
		maxAP = data["maxAP"]!.GetValue<int>();
		currentAP = maxAP;
		currentHP = maxHP;
		capacityCost = data["capacityCost"]!.GetValue<int>();
		goldCost = data["goldCost"]!.GetValue<int>();
		range = data["range"]!.GetValue<int>();
		damage = data["damage"]!.GetValue<int>();
		attackCost = data["attackCost"]!.GetValue<int>();

		string texString = (owner > 0) ? "enemyTexture" : "texture";
		Texture = ResourceLoader.Load<Texture2D>(data[texString]!.GetValue<string>());
	}

	public void setCurrentAP(int val)
	{
		currentAP = val;
		EmitSignal(SignalName.statsChanged);
	}

	public void setCurrentHP(int val)
	{
		currentHP = val;
		EmitSignal(SignalName.statsChanged);
	}

	public void applyDamage(int amount)
	{
		setCurrentHP(currentHP - amount);
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
