using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class HexGrid : Node2D
{
	public readonly int width, height;
	private HexCell[] grid;

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
	
	public bool indexInGrid(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	/**
	 * Deletes any existing cells and sets the cell in this grid to be the provided cell.
	 * Assumes the provided cell has been initialized with its grid position.
	 */
	public void setCell(HexCell c) {
		if (!indexInGrid(c.pos.X, c.pos.Y)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		deleteCell(c.pos.X, c.pos.Y);
		
		grid[c.pos.Y * width + c.pos.X] = c;
		AddChild(c);
	}
	
	public HexCell getCell(int x, int y) {
		if (!indexInGrid(x, y)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}
		
		return grid[y * width + x];
	}

	public HexCell getCell(Vector2I pos) {
		return getCell(pos.X, pos.Y);
	}

	public void deleteCell(int x, int y) {
		if (!indexInGrid(x, y)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}

		HexCell target = grid[y * width + x];
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

	public static (int x, int y) cubeToOffset(Vector3I c) {
		int col = c.X;
		int row = c.Y + (c.X + (c.X & 1)) / 2;
		return (col, row);
	}

	public List<HexCell> getNeighbors(HexCell cell) {
		return getNeighbors(cell.pos);
	}
	
	public List<HexCell> getNeighbors(Vector2I pos) {
		List<HexCell> neighbors = new List<HexCell>();

		for (int dx = -1; dx < 2; dx++) {
			for (int dy = -1; dy < 2; dy++) {
				int xi = pos.X + dx;
				int yi = pos.Y + dy;

				bool isSelf = dx == 0 && dy == 0;

				// Mask non-adjacent cells in 3x3 sweep
				bool isTopDiags = dx != 0 && dy == 1;
				bool isBottomDiags = dx != 0 && dy == -1;
				bool excludeDiags = (pos.X % 2 == 0) ? isBottomDiags : isTopDiags;
				
				if (!indexInGrid(xi, yi) || isSelf || excludeDiags) {
					continue;
				}
				
				neighbors.Add(getCell(xi, yi));
			}
		}
		return neighbors;
	}

	public static int hexDistance(Vector3I c1, Vector3I c2) {
		Vector3I vec = c1 - c2;
		return (Math.Abs(vec.X) + Math.Abs(vec.Y) + Math.Abs(vec.Z)) / 2;
	}

	public static int hexDistance(int x1, int y1, int x2, int y2) {
		Vector3I c1 = offsetToCube(x1, y1);
		Vector3I c2 = offsetToCube(x2, y2);
		return hexDistance(c1, c2);
	}
	
	public static int hexDistance(Vector2I cell1, Vector2I cell2) {
		Vector3I c1 = offsetToCube(cell1.X, cell1.Y);
		Vector3I c2 = offsetToCube(cell2.X, cell2.Y);
		return hexDistance(c1, c2);
	}

	public static int hexDistance(HexCell cell1, HexCell cell2) {
		return hexDistance(cell1.pos.X, cell1.pos.Y, cell2.pos.X, cell2.pos.Y);
	}

	public List<HexCell> getCellsInRadius(Vector2I center, int radius) {
		List<HexCell> cells = new List<HexCell>();
		Vector3I cubeCenter = offsetToCube(center.X, center.Y);

		for (int dq = -radius; dq <= radius; dq++) {
			for (int dr = Math.Max(-radius, -dq - radius); dr <= Math.Min(radius, -dq + radius); dr++) {
				int ds = -dq - dr;
				Vector3I cube = cubeCenter + new Vector3I(dq, dr, ds);
				var (x, y) = cubeToOffset(cube);

				if (indexInGrid(x, y)) {
					cells.Add(getCell(x, y));
				}
			}
		}

		return cells;
	}
}
