using Godot;
using System;
using System.Collections.Generic;

public class CityAction
{
	public string id;
	public string label;
	// public string keyBinding;
	public bool isAvailable;
	public Action onTrigger;
}

public partial class City : PlayerDecorator
{
	public List<Vector2I> ownedCells { get; private set; } = new List<Vector2I>();
	public List<CityAction> actions { get; private set; } = new List<CityAction>();
	public int goldProduction = 5;
	public string cityName = "City";

	public int maxHP = 100;
	public int currentHP;

	[Signal]
	public delegate void statsChangedEventHandler();

	[Signal]
	public delegate void actionsChangedEventHandler();
	
	public City(int owner, Vector2I pos) : base(owner) {
		Texture = ResourceLoader.Load<Texture2D>("res://assets/city.png");
		float scale = (float)(MapController.instance.hexSize * Math.Sqrt(3)) / Texture.GetHeight();
		SetScale(new Vector2(scale, scale));
		gridPosition = pos;
		ZIndex = 8;
		currentHP = maxHP;
	}
	
	public void addAction(CityAction action)
	{
		actions.Add(action);
	}
	
	/** Updates the availability of this city's actions. */
	public void updateAvailability(string id, bool available)
	{
		CityAction action = actions.Find(a => a.id == id);
		if (action != null)
		{
			action.isAvailable = available;
			EmitSignal(SignalName.actionsChanged);
		}
	}
}
