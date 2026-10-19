using Godot;
using System;
using System.Collections.Generic;

public class City : PlayerDecorator, IHasHealth
{
	public List<Vector2I> ownedCells { get; private set; } = new List<Vector2I>(); 
	public int goldProduction = 1;
	public string cityName = "City";
	public int maxHP { get; set; } = 2;
	public int currentHP { get; set; }

	public override string displayName => cityName;

	public City(int owner, Vector2I pos) : base(owner) {
		this.pos = pos;
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

public class Capital : City
{
	public Capital(int owner, Vector2I pos) : base(owner, pos) {
		cityName = "Capital";
		maxHP = 5;
		currentHP = maxHP;
		goldProduction = 3;
	}
}
