using Godot;
using System;
using System.Collections.Generic;

public partial class City : PlayerDecorator
{
	private List<Vector2I> ownedCells = new List<Vector2I>();
	private int level = 1;
	public int goldProduction = 5;
	
	public City(int owner, Vector2I pos) : base(owner) {
		Texture = ResourceLoader.Load<Texture2D>("res://assets/city.png");
		float scale = (float)(MapController.instance.hexSize * Math.Sqrt(3)) / Texture.GetHeight();
		SetScale(new Vector2(scale, scale));
		gridPosition = pos;
		ZIndex = 8;
	}
}
