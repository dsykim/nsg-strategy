using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public enum TerrainTypes
{
	PLAINS,
	HILLS,
	MOUNTAIN,
	OCEAN,
	EMPTY
}

public partial class MapController : Node2D
{
	public static MapController instance { get; private set; }
	private HexGrid hexGrid;
	private EdgeOverlay targetOverlay;
	public float hexSize { get; private set; }

	public readonly TerrainTypes[] impassableTerrain =
	{
			TerrainTypes.EMPTY,
			TerrainTypes.OCEAN,
			TerrainTypes.MOUNTAIN
	};

	public readonly TerrainTypes[] landTerrain =
	{
			TerrainTypes.HILLS,
			TerrainTypes.PLAINS,
			TerrainTypes.MOUNTAIN
	};
	
	Texture2D oceanTile = ResourceLoader.Load<Texture2D>("res://assets/waterHex.png");
	Texture2D plainTile = ResourceLoader.Load<Texture2D>("res://assets/plainHex.png");
	Texture2D hillTile = ResourceLoader.Load<Texture2D>("res://assets/hillHex.png");
	Texture2D mountainTile = ResourceLoader.Load<Texture2D>("res://assets/mountainHex.png");

	public MapController(int width, int height, float hexSize) {
		hexGrid = new HexGrid(width, height);
		AddChild(hexGrid);
		targetOverlay = new EdgeOverlay();
		AddChild(targetOverlay);

		this.hexSize = hexSize;
		instance = this;
		Name = "MapController";
	}

	public void addUnit(Unit unit) {
		Vector2I target = unit.gridPosition;
		HexCell c = hexGrid.getCell(target);
		c.units.Add(unit);
	}

	public void removeUnit(Unit unit) {
		HexCell c = hexGrid.getCell(unit.gridPosition);
		c.units.Remove(unit);
	}

	public bool canPlaceUnit(Vector2I target) {
		HexCell targetCell = hexGrid.getCell(target);
		return !targetCell.hasUnit() && !impassableTerrain.Contains(targetCell.terrainType);
	}

	public bool canMoveUnit(Unit unit, Vector2I target) {
		int moveDist = HexGrid.hexDistance(unit.gridPosition, target);
		return moveDist <= unit.currentAP && canPlaceUnit(target);
	}

	public List<Vector2I> getMovableCells(Unit unit) {
		List<Vector2I> reachable = new List<Vector2I>();

		var visited = new HashSet<Vector2I> { unit.gridPosition };
		var frontier = new Queue<(HexCell cell, int cost)>();
		frontier.Enqueue((hexGrid.getCell(unit.gridPosition), 0));

		while (frontier.Count > 0) {
			var (cell, cost) = frontier.Dequeue();
			if (cost >= unit.currentAP) continue; // no AP left to step further

			foreach (HexCell neighbor in hexGrid.getNeighbors(cell)) {
				if (!visited.Add(neighbor.pos)) continue; // first reach = shortest (unit weights)

				if (impassableTerrain.Contains(neighbor.terrainType)) continue;
				bool canStop = !neighbor.hasUnit();
				bool canTraverse = canStop || unit.owner == neighbor.units[0].owner;

				if (canStop) reachable.Add(neighbor.pos);
				if (canTraverse) frontier.Enqueue((neighbor, cost + 1));
			}
		}

		return reachable;
	}

	public List<Vector2I> getCellsInRadius(Vector2I target, int radius) {
		var hexCellsInRadius = hexGrid.getCellsInRadius(target, radius);
		var cellPosInRadius = new List<Vector2I>();
		foreach (HexCell c in hexCellsInRadius) {
			cellPosInRadius.Add(c.pos);
		}
		return cellPosInRadius;
	}

	public int pathDistance(Vector2I start, Vector2I goal) {
		if (start == goal) return 0;

		var visited = new HashSet<Vector2I> { start };
		var frontier = new Queue<(Vector2I pos, int cost)>();
		frontier.Enqueue((start, 0));

		while (frontier.Count > 0) {
			var (pos, cost) = frontier.Dequeue();

			foreach (HexCell neighbor in hexGrid.getNeighbors(pos)) {
				if (!visited.Add(neighbor.pos)) continue; // first reach = shortest (unit weights)
				if (impassableTerrain.Contains(neighbor.terrainType)) continue;

				if (neighbor.pos == goal) return cost + 1;
				frontier.Enqueue((neighbor.pos, cost + 1));
			}
		}

		return -1; // no walkable path exists
	}

	public void moveUnit(Unit unit, Vector2I target) {
		if (canMoveUnit(unit, target)) {
			removeUnit(unit);
			unit.currentAP -= pathDistance(unit.gridPosition, target);
			unit.gridPosition = target;
			unit.SetPosition(getCellCenter(target));
			addUnit(unit);
		} else {
			Debug.Print("Cannot move unit to " + target);
		}
	}

	/**
	 * Can place a city if the target is at least two tiles from another city, not in controlled territory,
	 * and on passable terrain.
	 */
	public bool canPlaceCity(Vector2I target) {
		HexCell targetCell = hexGrid.getCell(target);
		bool isNotControlled = !targetCell.hasController();
		bool onPassableTerrain = !impassableTerrain.Contains(targetCell.terrainType);
		bool withinRangeOfCity = false;
		foreach (HexCell cell in hexGrid.getCellsInRadius(target, 2)) {
			if (cell.hasCity()) {
				withinRangeOfCity = true;
				break;
			}
		}
		
		return isNotControlled && onPassableTerrain && !withinRangeOfCity;
	}

	/** Adds a city to its cell and assigns control of all cells in the city to its controller. */
	public void addCity(City city) {
		HexCell c = hexGrid.getCell(city.gridPosition);
		c.city = city;
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
				tex = plainTile;
				break;
			case TerrainTypes.HILLS:
				tex = hillTile;
				break;
			case TerrainTypes.MOUNTAIN:
				tex = mountainTile;
				break;
			default:
				tex = oceanTile;
				break;
		}
		cell.SetTexture(tex);
		cell.SetScale(new Vector2(2 * hexSize, (float)Math.Sqrt(3) * hexSize) / tex.GetSize());

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

	public List<Vector2I> getNeighborPositions(Vector2I target) {
		List<HexCell> neighbors = hexGrid.getNeighbors(target);
		List<Vector2I> positions = new List<Vector2I>();
		foreach (HexCell c in neighbors) {
			positions.Add(c.pos);
		}
		return positions;
	}

	static readonly (int a, int b)[] edgeVerts =
	{
			(4, 5), // N
			(5, 0), // NE
			(0, 1), // SE
			(1, 2), // S
			(2, 3), // SW
			(3, 4), // NW
	};

	public Vector2[] getCellVertices(Vector2I pos) {
		Vector2 c = getCellCenter(pos);
		float s = hexSize, h = (float)Math.Sqrt(3) * hexSize / 2f;
		return new[]
		{
				c + new Vector2(s, 0), c + new Vector2(s / 2, h), c + new Vector2(-s / 2, h),
				c + new Vector2(-s, 0), c + new Vector2(-s / 2, -h), c + new Vector2(s / 2, -h),
		};
	}

	public int getCellOwner(Vector2I pos) {
		return hexGrid.getCell(pos).controllerID;
	}

	public void setCellOwner(Vector2I pos, int id) {
		hexGrid.getCell(pos).controllerID = id;
	}

	public (Vector2 p1, Vector2 p2) getEdgeEndpoints(Vector2I pos, HexDirection dir) {
		Vector2[] v = getCellVertices(pos);
		var (a, b) = edgeVerts[(int)dir];
		return (v[a], v[b]);
	}

	public List<(Vector2, Vector2)> getRegionOutline(IEnumerable<Vector2I> region) {
		var inSet = new HashSet<Vector2I>();
		foreach (var c in region) inSet.Add(c);

		var segs = new List<(Vector2, Vector2)>();
		foreach (var pos in inSet)
		foreach (HexDirection d in Enum.GetValues<HexDirection>())
			if (!inSet.Contains(HexGrid.neighbor(pos, d)))
				segs.Add(getEdgeEndpoints(pos, d));
		return segs;
	}

	public void showTargetRegion(IEnumerable<Vector2I> cells, Color color) {
		targetOverlay.LineColor = color;
		targetOverlay.SetSegments(getRegionOutline(cells));
	}

	public void clearTargetRegion() => targetOverlay.Clear();

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
			bool isBorder = next.pos.X == 0 ||
							next.pos.X == hexGrid.width - 1 ||
							next.pos.Y == 0 ||
							next.pos.Y == hexGrid.height - 1;
			bool isBorderAdj = next.pos.X == 1 ||
							   next.pos.X == hexGrid.width - 2 ||
							   next.pos.Y == 1 ||
							   next.pos.Y == hexGrid.height - 2;
			int distToSeed = HexGrid.hexDistance(next.pos, seed.pos);
			float threshold = Math.Max(0.95f - (float)Math.Pow((float)distToSeed / hexGrid.width, 2), 0.25f);

			if (isBorderAdj) threshold = 0.2f;

			if (!isBorder && rand.NextSingle() < threshold) {
				// Make land
				float randVal = rand.NextSingle();
				TerrainTypes lType;
				if (randVal < 0.6) {
					lType = TerrainTypes.PLAINS;
				} else if (randVal < 0.9) {
					lType = TerrainTypes.HILLS;
				} else {
					lType = TerrainTypes.MOUNTAIN;
				}
				
				HexCell generated = createCell(next.pos, lType);
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
				Vector2I pos = new Vector2I(x, y);
				if (hexGrid.getCell(pos).terrainType == TerrainTypes.EMPTY) {
					HexCell c = createCell(pos, TerrainTypes.OCEAN);
					hexGrid.setCell(c);
				}
			}
		}
	}

}
