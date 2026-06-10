using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class MapController : Node2D
{
	public static MapController instance { get; private set; }
	private HexGrid hexGrid;
	public float hexSize { get; private set; }

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
		Name = "MapController";
	}
	public void addUnit(Unit unit, Vector2I target) {
		HexCell c = hexGrid.getCell(target);
		if (canPlaceUnit(unit, target)) {
			unit.SetPosition(getCellCenter(target));
			unit.gridPosition = target;
			c.units.Add(unit);
			if (unit.GetParent() == null) {
				AddChild(unit);
			}
		} else {
			Debug.Print("Cannot place unit at "+ target);
		}
	}

	public void removeUnit(Unit unit) {
		HexCell c = hexGrid.getCell(unit.gridPosition.X, unit.gridPosition.Y);
		c.units.Remove(unit);
		RemoveChild(unit);
	}

	public bool canPlaceUnit(Unit unit, Vector2I target) {
		HexCell targetCell = hexGrid.getCell(target);
		return !targetCell.hasUnit() && !impassableTerrain.Contains(targetCell.terrainType);
	}
	public bool canMoveUnit(Unit unit, Vector2I target) {
		int moveDist = HexGrid.hexDistance(unit.gridPosition, target);
		return moveDist <= unit.currentAP && canPlaceUnit(unit, target);
	}
	
	public void moveUnit(Unit unit, Vector2I target) {
		HexCell currentCell = hexGrid.getCell(unit.gridPosition);
		HexCell targetCell = hexGrid.getCell(target);
		if (canMoveUnit(unit, target)) {
			currentCell.units.Remove(unit);
			unit.move(target);
			unit.SetPosition(getCellCenter(target));
			targetCell.units.Add(unit);
		} else {
			Debug.Print("Cannot move unit to " + target);
		}
	}

	public bool canPlaceCity(City c, Vector2I target) {
		HexCell targetCell = hexGrid.getCell(target);
		return !targetCell.hasCity() && !impassableTerrain.Contains(targetCell.terrainType);
	}
	
	public void addCity(City city, Vector2I target) {
		HexCell c = hexGrid.getCell(target);
		if (canPlaceCity(city, target)) {
			city.SetPosition(getCellCenter(target));
			city.gridPosition = target;
			c.city = city;
			AddChild(city);
		} else {
			Debug.Print("Cannot place city at " + target);
		}
	}

	public void addNaturalDecorator(NaturalDecorator dec, Vector2I target) {
		
	}

	public void addPlayerDecorator(PlayerDecorator dec, Vector2I target) {
		
	}

	private HexCell createCell(Vector2I pos, TerrainTypes tType) {
		
		HexCell cell = new HexCell(pos, tType);
		
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

		Vector2 center = getCellCenter(pos);
		cell.SetPosition(center);
		cell.addHexCollision();
		return cell;
	}

	public Vector2I getGridSize() {
		return new Vector2I(hexGrid.width, hexGrid.height);
	}
	
	/**
	 * Returns the world space coordinates of the center of the specified cell.
	 */
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

	public List<HexCell> getNeighbors(Vector2I target) {
		return hexGrid.getNeighbors(target);
	}

	public List<Vector2I> getNeighborPositions(Vector2I target) {
		List<HexCell> neighbors = hexGrid.getNeighbors(target);
		List<Vector2I> positions = new List<Vector2I>();
		foreach (HexCell c in neighbors) {
			positions.Add(c.pos);
		}

		return positions;
	}
	
	public void generateMap() {
		Debug.Print("Generating Map...");

		Queue<HexCell> frontier = new Queue<HexCell>();
		
		HexCell seed = createCell(new Vector2I(hexGrid.width / 2, hexGrid.height / 2), 
				TerrainTypes.PLAINS);
		hexGrid.setCell(seed);
		foreach (HexCell c in hexGrid.getNeighbors(seed)) {
			frontier.Enqueue(c);
		}

		Random rand = new Random();
		while (frontier.Count > 0) {
			HexCell next = frontier.Dequeue();
			bool isBorder = next.pos.X == 0 || next.pos.X == hexGrid.width - 1 || next.pos.Y == 0 || next.pos.Y == hexGrid.height - 1;
			bool isBorderAdj = next.pos.X == 1 || next.pos.X == hexGrid.width - 2 || next.pos.Y == 1 || next.pos.Y == hexGrid.height - 2;
			int distToSeed = HexGrid.hexDistance(next, seed);
			float threshold = Math.Max(0.95f - (float)Math.Pow((float)distToSeed / hexGrid.width, 2), 0.25f);

			if (isBorderAdj) threshold = 0.2f;
			
			if (!isBorder && rand.NextSingle() < threshold) {
				// Make land
				HexCell generated = createCell(next.pos, TerrainTypes.PLAINS);
				hexGrid.setCell(generated);
				foreach (HexCell c in hexGrid.getNeighbors(generated)) {
					if (!frontier.Contains(c) && c.terrainType == TerrainTypes.EMPTY) {
						frontier.Enqueue(c);
					}
				}
			} else {
				// Make ocean
				HexCell generated = createCell(next.pos, TerrainTypes.OCEAN);
				hexGrid.setCell(generated);
			}
		}

		for (int x = 0; x < hexGrid.width; x++) {
			for (int y = 0; y < hexGrid.height; y++) {
				if (hexGrid.getCell(x, y).terrainType == TerrainTypes.EMPTY) {
					HexCell c = createCell(new Vector2I(x, y), TerrainTypes.OCEAN);
					hexGrid.setCell(c);
				}
			}
		}
	}

}
