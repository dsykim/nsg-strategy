using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class MapController : Node2D
{
	public static MapController instance { get; private set; }
	private HexGrid hexGrid;
	private float hexSize;
	
	public readonly TerrainTypes[] impassableTerrain = { 
			TerrainTypes.EMPTY, 
			TerrainTypes.OCEAN, 
			TerrainTypes.MOUNTAIN 
	};
	
	Texture2D oceanTile = ResourceLoader.Load<Texture2D>("res://assets/water_hex.png");
	Texture2D landTile = ResourceLoader.Load<Texture2D>("res://assets/ground_hex.png");

	public MapController(int width, int height, float hexSize) {
		hexGrid = new HexGrid(width, height);
		AddChild(hexGrid);
		this.hexSize = hexSize;
		instance = this;
	}

	public void addUnit(Unit unit, int x, int y) {
		HexCell c = hexGrid.getCell(x, y);
		if (c.units.Count == 0) {
			unit.SetPosition(getCellCenter(x, y));
			unit.gridPosition = new Vector2I(x, y);
			c.units.Add(unit);
			if (unit.GetParent() == null) {
				AddChild(unit);
			}
		}
	}

	public void removeUnit(Unit unit) {
		HexCell c = hexGrid.getCell(unit.gridPosition.X, unit.gridPosition.Y);
		c.units.Remove(unit);
		RemoveChild(unit);
	}

	public bool canMove(Unit unit, Vector2I target) {
		HexCell targetCell = hexGrid.getCell(target);
		int moveDist = HexGrid.hexDistance(unit.gridPosition.X, unit.gridPosition.Y, target.X, target.Y);

		
		return moveDist <= unit.currentAP && targetCell.units.Count == 0
				&& !impassableTerrain.Contains(targetCell.terrainType);
	}
	
	public void moveUnit(Unit unit, Vector2I target) {
		HexCell currentCell = hexGrid.getCell(unit.gridPosition);
		HexCell targetCell = hexGrid.getCell(target);
		if (canMove(unit, target)) {
			currentCell.units.Remove(unit);
			unit.move(target);
			unit.SetPosition(getCellCenter(target));
			targetCell.units.Add(unit);
		}
	}

	public void addCity(City city, int x, int y) {
		HexCell c = hexGrid.getCell(x, y);
		if (c.city == null) {
			city.SetPosition(getCellCenter(x, y));
			city.gridPosition = new Vector2I(x, y);
			c.city = city;
			AddChild(city);
		}
	}

	public void addNaturalDecorator(NaturalDecorator dec, int x, int y) {
		
	}

	public void addPlayerDecorator(PlayerDecorator dec, int x , int y) {
		
	}

	public void generateMap() {
		Debug.Print("Generating Map...");

		Queue<HexCell> frontier = new Queue<HexCell>();
		
		HexCell seed = createCell(hexGrid.width / 2, hexGrid.height / 2, TerrainTypes.PLAINS);
		hexGrid.setCell(seed);
		foreach (HexCell c in hexGrid.getNeighbors(seed)) {
			frontier.Enqueue(c);
		}

		Random rand = new Random();
		while (frontier.Count > 0) {
			HexCell next = frontier.Dequeue();
			bool isBorder = next.x == 0 || next.x == hexGrid.width - 1 || next.y == 0 || next.y == hexGrid.height - 1;
			bool isBorderAdj = next.x == 1 || next.x == hexGrid.width - 2 || next.y == 1 || next.y == hexGrid.height - 2;
			int distToSeed = HexGrid.hexDistance(next, seed);
			float threshold = Math.Max(0.95f - (float)Math.Pow((float)distToSeed / hexGrid.width, 2), 0.25f);

			if (isBorderAdj) threshold = 0.2f;
			
			if (!isBorder && rand.NextSingle() < threshold) {
				// Make land
				HexCell generated = createCell(next.x, next.y, TerrainTypes.PLAINS);
				hexGrid.setCell(generated);
				foreach (HexCell c in hexGrid.getNeighbors(generated)) {
					if (!frontier.Contains(c) && c.terrainType == TerrainTypes.EMPTY) {
						frontier.Enqueue(c);
					}
				}
			} else {
				// Make ocean
				HexCell generated = createCell(next.x, next.y, TerrainTypes.OCEAN);
				hexGrid.setCell(generated);
			}
		}

		for (int x = 0; x < hexGrid.width; x++) {
			for (int y = 0; y < hexGrid.height; y++) {
				if (hexGrid.getCell(x, y).terrainType == TerrainTypes.EMPTY) {
					HexCell c = createCell(x, y, TerrainTypes.OCEAN);
					hexGrid.setCell(c);
				}
			}
		}
	}

	private HexCell createCell(int x, int y, TerrainTypes tType) {
		
		HexCell cell = new HexCell(x, y, tType);
		
		Texture2D tex;
		switch (tType) {
			case TerrainTypes.OCEAN:
				tex = oceanTile;
				break;
			case TerrainTypes.PLAINS:
				tex = landTile;
				break;
			default:
				tex = oceanTile;
				break;
		}
		cell.SetTexture(tex);
		cell.SetScale(new Vector2(2*hexSize, (float)Math.Sqrt(3) * hexSize) / tex.GetSize());

		Vector2 center = getCellCenter(x, y);
		cell.SetPosition(center);
		return cell;
	}

	public Vector2 getCellCenter(int x, int y) {
		float hexWidth = 2 * hexSize;
		float hexHeight = (float)Math.Sqrt(3) * hexSize;
		float hOffset = hexWidth / 2f;
		float vOffset = (x & 1) == 0 ? hexHeight : hexHeight / 2f;
		float hStep = 3f / 4 * hexWidth;
		float vStep = (float)Math.Sqrt(3) * hexSize;

		return new Vector2(hOffset + x * hStep, vOffset + y * vStep);
	}
	
	public Vector2 getCellCenter(Vector2I v) {
		return getCellCenter(v.X, v.Y);
	}

}
