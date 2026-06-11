using Godot;
using System.Collections.Generic;

public abstract partial class Unit : CellDecorator
{
	public int maxHP;
	public int currentHP;
	public int maxAP;
	public int currentAP;

	/** ID of the player who owns this unit. */
	public readonly int owner;

	/** A list of this unit's actions. */
	public List<UnitAction> actions { get; private set; } = new List<UnitAction>();

	/** Signal triggered when action availability is updated. */
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
