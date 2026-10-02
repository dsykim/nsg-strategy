using Godot;
using System;
using System.Collections.Generic;

public class City : PlayerDecorator
{
	public List<Vector2I> ownedCells { get; private set; } = new List<Vector2I>(); 
	public int goldProduction = 5;
	public string cityName = "City";
	public int maxHP = 100;
	public int currentHP;

	public City(int owner, Vector2I pos) : base(owner) {
		gridPosition = pos;
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
