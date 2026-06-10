using Godot;
using System.Collections.Generic;

public abstract partial class Unit : CellDecorator
{
	public int maxHP;
	public int currentHP;
	public int maxAP;
	public int currentAP;

	public readonly int owner;

	public List<UnitAction> actions { get; private set; } = new List<UnitAction>();

	[Signal] public delegate void actionsChangedEventHandler();

	public Unit(int owner)
	{
		this.owner = owner;
	}

	public void setCurrentAP(int val)
	{
		currentAP = val;
	}

	public void addAction(UnitAction action)
	{
		actions.Add(action);
	}

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
