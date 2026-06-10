using Godot;
using System;

public partial class City : PlayerDecorator
{
	public City(int owner) : base(owner) {
		Texture = ResourceLoader.Load<Texture2D>("res://assets/city.png");
		float scale = (float)(MapController.instance.hexSize * Math.Sqrt(3)) / Texture.GetHeight();
		SetScale(new Vector2(scale, scale));
	}
}
