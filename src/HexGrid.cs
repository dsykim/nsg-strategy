using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class HexGrid
{
	public readonly int width, height;
	public HexCell[] grid { get; private set; }
	private Dictionary<EdgeKey, HexEdge> edges = new();

	public HexGrid(int w, int h) {
		width = w;
		height = h;
		grid = new HexCell[w * h];
		for (int i = 0; i < w * h; i++) {
			Vector2I pos = new Vector2I(i % w, i / w);
			HexCell c = new HexCell(pos);
			grid[i] = c;
		}
	}

	private HexGrid(HexGrid other) {
		width = other.width;
		height = other.height;

		grid = new HexCell[other.grid.Length];
		for (int i = 0; i < grid.Length; i++)
			grid[i] = other.grid[i].clone();

		edges = new Dictionary<EdgeKey, HexEdge>(other.edges.Count);
		foreach (var (key, edge) in other.edges)
			edges[key] = edge.clone();
	}

	public HexGrid clone() => new HexGrid(this);
	
	public bool indexInGrid(Vector2I pos) {
		return pos.X >= 0 && pos.X < width && pos.Y >= 0 && pos.Y < height;
	}

	public void setCell(Vector2I pos, TerrainTypes tType = TerrainTypes.EMPTY) {
		if (!indexInGrid(pos)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}
		
		grid[pos.X + pos.Y * width].terrainType = tType;
	}

	public HexCell getCell(Vector2I pos) {
		if (!indexInGrid(pos)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		return grid[pos.Y * width + pos.X];
	}

	public List<HexCell> getNeighbors(Vector2I pos) {
		List<HexCell> neighbors = new List<HexCell>();

		foreach (HexDirection dir in Enum.GetValues<HexDirection>()) {
			Vector2I neighborPos = HexUtils.neighbor(pos, dir);
			if (indexInGrid(neighborPos)) {
				neighbors.Add(getCell(neighborPos));
			}
		}
		return neighbors;
	}

	public List<Vector2I> getNeighborPositions(Vector2I pos) {
		List<Vector2I> neighbors = new List<Vector2I>();

		foreach (HexDirection dir in Enum.GetValues<HexDirection>()) {
			Vector2I neighborPos = HexUtils.neighbor(pos, dir);
			if (indexInGrid(neighborPos)) {
				neighbors.Add(neighborPos);
			}
		}
		return neighbors;
	}
	
	public List<HexCell> getCellsInRadius(Vector2I center, int radius) {
		List<HexCell> cells = new List<HexCell>();
		Vector3I cubeCenter = HexUtils.offsetToCube(center.X, center.Y);

		for (int dq = -radius; dq <= radius; dq++) {
			for (int dr = Math.Max(-radius, -dq - radius); dr <= Math.Min(radius, -dq + radius); dr++) {
				int ds = -dq - dr;
				Vector3I cube = cubeCenter + new Vector3I(dq, dr, ds);
				var pos = HexUtils.cubeToOffset(cube);

				if (indexInGrid(pos)) {
					cells.Add(getCell(pos));
				}
			}
		}

		return cells;
	}
	
	public List<Vector2I> getCellsPosInRadius(Vector2I center, int radius) {
		List<Vector2I> cells = new List<Vector2I>();
		Vector3I cubeCenter = HexUtils.offsetToCube(center.X, center.Y);

		for (int dq = -radius; dq <= radius; dq++) {
			for (int dr = Math.Max(-radius, -dq - radius); dr <= Math.Min(radius, -dq + radius); dr++) {
				int ds = -dq - dr;
				Vector3I cube = cubeCenter + new Vector3I(dq, dr, ds);
				var pos = HexUtils.cubeToOffset(cube);

				if (indexInGrid(pos)) {
					cells.Add(pos);
				}
			}
		}

		return cells;
	}
	
	public List<Vector2I> getCellsInLine(Vector2I p1, Vector2I p2) {
		if (p1 == p2) {
			throw new Exception("Line endpoints must be different");
		}
		Vector3I c1 = HexUtils.offsetToCube(p1.X, p1.Y);
		Vector3I c2 = HexUtils.offsetToCube(p2.X, p2.Y);

		int N = HexUtils.hexDistance(c1, c2);
		List<Vector2I> results = new List<Vector2I>();
		for (int i = 0; i < N + 1; i++) {
			results.Add(HexUtils.cubeToOffset(HexUtils.cubeRound(HexUtils.cubeLerp(c1, c2, 1f / N * i))));
		}

		return results;
	}
	public HexEdge getEdge(Vector2I pos, HexDirection dir, bool create = true) {
		var key = new EdgeKey(pos, HexUtils.neighbor(pos, dir));
		if (edges.TryGetValue(key, out var e)) {
			return e;
		}
		if (!create) {
			return null;
		}
		e = new HexEdge(key);
		edges[key] = e;
		return e;
	}
	
	public List<Vector2I> getCellsInRadiusRange(Vector2I target, int minRadius, int maxRadius) {
		if (minRadius > maxRadius || minRadius < 1) {
			throw new Exception("Illegal radius");
		}

		List<Vector2I> cellsInRadius = getCellsPosInRadius(target, maxRadius);
		List<Vector2I> included = new List<Vector2I>();
		foreach (var cell in cellsInRadius) {
			if (HexUtils.hexDistance(target, cell) >= minRadius) {
				included.Add(cell);
			}
		}
		return included;
	}
}
