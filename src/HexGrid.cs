using Godot;
using System;
using System.Collections.Generic;

public class CubeCoord
{
	public int q, r, s;

	public CubeCoord(int q, int r, int s) {
		this.q = q;
		this.r = r;
		this.s = s;
	}
	public CubeCoord sub(CubeCoord other) {
		return new CubeCoord(q - other.q, r - other.r, s - other.s);
	}
}

public partial class HexGrid : Node2D
{
	private int width, height;

	private List<HexCell> grid;

	public HexGrid(int w, int h) {
		width = w;
		height = h;
		grid = new List<HexCell>(w * h);
	}
	
	public bool indexInGrid(int x, int y) {
		return x > +0 && x < width && y >= 0 && y < height;
	}
	
	public HexCell getCell(int x, int y) {
		if (!indexInGrid(x, y)) {
			throw new IndexOutOfRangeException("Cell coordinates out of grid range");
		}
		
		return grid[y * width + x];
	}

	/** Coordinate conversion between axial (x,y) and cube (q, r, s) coordinates. */
	public static (int x, int y) cubeToOffset(CubeCoord c) {
		var parity = c.r & 1;
		var col = c.q + (c.r + parity) / 2;
		var row = c.r;
		return (col, row);
	}
	
	/** Coordinate conversion between cube (q, r, s) and axial (x,y) coordinates. */
	public static CubeCoord offsetToCube(int x, int y) {
		var parity = y & 1;
		var q = x - (y + parity) / 2;
		var r = y;
		return new CubeCoord(q, r, -q-r);
	}

	public List<HexCell> getNeighbors(HexCell cell) {
		return getNeighbors(cell.x, cell.y);
	}
	
	public List<HexCell> getNeighbors(int x, int y) {
		List<HexCell> neighbors = new List<HexCell>();

		for (int dx = -1; dx < 2; dx++) {
			for (int dy = -1; dy < 2; dy++) {
				int xi = x + dx;
				int yi = y + dy;

				bool isSelf = dx == 0 && dy == 0;

				// Mask non-adjacent cells in 3x3 sweep
				bool isTopDiags = dx != 0 && dy == 1;
				bool isBottomDiags = dx != 0 && dy == -1;
				bool excludeDiags = (x % 2 == 0) ? isBottomDiags : isTopDiags;
				
				if (!indexInGrid(xi, yi) || isSelf || excludeDiags) {
					continue;
				}
				
				neighbors.Add(getCell(xi, yi));
			}
		}
		return neighbors;
	}

	public int hexDistance(CubeCoord c1, CubeCoord c2) {
		var vec = c1.sub(c2);
		return (Math.Abs(vec.q) + Math.Abs(vec.r) + Math.Abs(vec.s)) / 2;
	}

	public int hexDistance(int x1, int y1, int x2, int y2) {
		CubeCoord c1 = offsetToCube(x1, y1);
		CubeCoord c2 = offsetToCube(x2, y2);
		return hexDistance(c1, c2);
	}

	public int hexDistance(HexCell cell1, HexCell cell2) {
		return hexDistance(cell1.x, cell1.y, cell2.x, cell2.y);
	}
}
