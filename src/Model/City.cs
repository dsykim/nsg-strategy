using Godot;
using System;
using System.Collections.Generic;

public class City : PlayerDecorator, IHasHealth
{
	public List<Vector2I> ownedCells { get; private set; } = new List<Vector2I>(); 
	public int goldProduction = 1;
	public string cityName = "City";
	public int maxHP { get; set; } = 1;
	public int currentHP { get; set; }

	public override string displayName => cityName;

	public City(int owner, Vector2I pos) : base(owner) {
		((CellDecorator)this).pos = pos;
		currentHP = maxHP;
	}

	public override City clone() {
		City clone = (City)MemberwiseClone();
		clone.ownedCells = new List<Vector2I>(ownedCells);
		return clone;
	}

	public bool isDead() => currentHP <= 0;
	
	public void applyDamage(int amount) => currentHP -= amount;
}
