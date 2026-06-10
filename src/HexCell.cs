using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class HexCell : Sprite2D
{
	public readonly Vector2I pos;
	public TerrainTypes terrainType;

	public NaturalDecorator naturalDecorator = null;
	public PlayerDecorator playerDecorator = null;
	public City city = null;
	public List<Unit> units = new List<Unit>();

	public HexCell(Vector2I pos) {
		this.pos = pos;
		terrainType = TerrainTypes.EMPTY;
		Name = $"Cell_{pos.X}_{pos.Y}";
	}

	public HexCell(Vector2I pos, TerrainTypes tType) : this(pos) {
		terrainType = tType;
		switch (terrainType) {
			case TerrainTypes.OCEAN:
				break;
			case TerrainTypes.HILLS:

			case TerrainTypes.PLAINS:

			case TerrainTypes.MOUNTAIN:

			case TerrainTypes.EMPTY:
				break;
		}
	}

	public bool hasCity() {
		return city != null;
	}

	public bool hasUnit() {
		return units.Count > 0;
	}

	public bool hasNaturalDecorator() {
		return naturalDecorator != null;
	}

	public bool hasPlayerDecorator() {
		return playerDecorator != null;
	}

	public void addHexCollision() {
		var size = Texture.GetSize();
		float width = size.X / 2f;
		float height = size.Y / 2f;

		var hexPoints = new Vector2[]
		{
				new Vector2(width, 0),
				new Vector2(width / 2f, height),
				new Vector2(-width / 2f, height),
				new Vector2(-width, 0),
				new Vector2(-width / 2f, -height),
				new Vector2(width / 2f, -height),
		};

		var shape = new CollisionPolygon2D();
		shape.Polygon = hexPoints;

		var area = new Area2D();
		area.Monitoring = true;
		area.Monitorable = true;
		area.AddChild(shape);
		AddChild(area);
	}
}
