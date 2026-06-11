using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum HexDirection
{
	N,
	NE,
	SE,
	S,
	SW,
	NW
}

public partial class HexGrid : Node2D
{
	public readonly int width, height;
	private HexCell[] grid;
	private Dictionary<EdgeKey, HexEdge> edges = new();

	public HexGrid(int w, int h) {
		width = w;
		height = h;
		grid = new HexCell[w * h];
		for (int i = 0; i < w * h; i++) {
			HexCell c = new HexCell(new Vector2I(i % w, i / w));
			setCell(c);
		}
		Name = "HexGrid";
	}

	static readonly Vector2I[] evenColOffsets =
	{
			new(0, -1), new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0)
	};
	static readonly Vector2I[] oddColOffsets =
	{
			new(0, -1), new(1, -1), new(1, 0), new(0, 1), new(-1, 0), new(-1, -1)
	};

	public static Vector2I neighbor(Vector2I pos, HexDirection dir) =>
			pos + ((pos.X & 1) == 0 ? evenColOffsets : oddColOffsets)[(int)dir];

	public static HexDirection opposite(HexDirection dir) => (HexDirection)(((int)dir + 3) % 6);

	public bool indexInGrid(Vector2I pos) {
		return pos.X >= 0 && pos.X < width && pos.Y >= 0 && pos.Y < height;
	}

	/**
	 * Deletes any existing cells and sets the cell in this grid to be the provided cell.
	 * Assumes the provided cell has been initialized with its grid position.
	 */
	public void setCell(HexCell c) {
		if (!indexInGrid(c.pos)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		deleteCell(c.pos);

		grid[c.pos.Y * width + c.pos.X] = c;
		AddChild(c);
	}

	public HexCell getCell(Vector2I pos) {
		if (!indexInGrid(pos)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		return grid[pos.Y * width + pos.X];
	}

	public HexEdge getEdge(Vector2I pos, HexDirection dir, bool create = true) {
		var key = new EdgeKey(pos, neighbor(pos, dir));
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

	public void deleteCell(Vector2I pos) {
		if (!indexInGrid(pos)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		HexCell target = grid[pos.Y * width + pos.X];
		if (target != null) {
			RemoveChild(target);
			target.QueueFree();
		}
	}

	public static Vector3I offsetToCube(int x, int y) {
		int q = x;
		int r = y - (x + (x & 1)) / 2;
		return new Vector3I(q, r, -q - r);
	}

	public static Vector2I cubeToOffset(Vector3I c) {
		int col = c.X;
		int row = c.Y + (c.X + (c.X & 1)) / 2;
		return new Vector2I(col, row);
	}

	public List<HexCell> getNeighbors(HexCell cell) {
		return getNeighbors(cell.pos);
	}

	public List<HexCell> getNeighbors(Vector2I pos) {
		List<HexCell> neighbors = new List<HexCell>();

		foreach (HexDirection dir in Enum.GetValues<HexDirection>()) {
			Vector2I neighborPos = neighbor(pos, dir);
			if (indexInGrid(neighborPos)) {
				neighbors.Add(getCell(neighborPos));
			}
		}
		return neighbors;
	}

	public static int hexDistance(Vector3I c1, Vector3I c2) {
		Vector3I vec = c1 - c2;
		return (Math.Abs(vec.X) + Math.Abs(vec.Y) + Math.Abs(vec.Z)) / 2;
	}

	public static int hexDistance(Vector2I cell1, Vector2I cell2) {
		Vector3I c1 = offsetToCube(cell1.X, cell1.Y);
		Vector3I c2 = offsetToCube(cell2.X, cell2.Y);
		return hexDistance(c1, c2);
	}

	public List<HexCell> getCellsInRadius(Vector2I center, int radius) {
		List<HexCell> cells = new List<HexCell>();
		Vector3I cubeCenter = offsetToCube(center.X, center.Y);

		for (int dq = -radius; dq <= radius; dq++) {
			for (int dr = Math.Max(-radius, -dq - radius); dr <= Math.Min(radius, -dq + radius); dr++) {
				int ds = -dq - dr;
				Vector3I cube = cubeCenter + new Vector3I(dq, dr, ds);
				var pos = cubeToOffset(cube);

				if (indexInGrid(pos)) {
					cells.Add(getCell(pos));
				}
			}
		}

		return cells;
	}
}
