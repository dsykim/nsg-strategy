using Godot;

/**
 * Converts model grid coordinates to view-space pixels.
 * This is the only place in the view layer that knows hex geometry; every layer goes through it.
 *
 * Geometry is defined by the tile's bounding box (cellSize) rather than a radius, so it can be
 * taken straight from the tile texture and art lines up pixel-for-pixel even when the texture
 * is not a mathematically regular hex. Spacing assumes the standard hex overlap:
 *   pointy-top: columns step by width,      rows step by 3/4 height
 *   flat-top:   columns step by 3/4 width,  rows step by height
 *
 * Positions are derived from HexUtils.offsetToCube, so the model's offset convention is respected
 * automatically. Only the orientation has to match it: row-offset conventions are pointy-top,
 * column-offset conventions are flat-top.
 */
public sealed class HexLayout
{
	private static readonly float Sqrt3 = Mathf.Sqrt(3f);

	public readonly Vector2 cellSize;
	public readonly Vector2 origin;

	private readonly Vector2[] cornerOffsets = new Vector2[6];

	public HexLayout(Vector2 cellSize, Vector2 origin) {
		this.cellSize = cellSize;
		this.origin = origin;

		float startDeg = 0f;
		var stretch = new Vector2(cellSize.X / 2f, cellSize.Y / Sqrt3);

		for (int i = 0; i < 6; i++) {
			float a = Mathf.DegToRad(startDeg + 60f * i);
			cornerOffsets[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * stretch;
		}
	}

	public HexLayout withOrigin(Vector2 newOrigin) => new HexLayout(cellSize, newOrigin);

	/** Center of a cell, in the board's local space. */
	public Vector2 cellToWorld(Vector2I cell) {
		Vector3I cube = HexUtils.offsetToCube(cell.X, cell.Y);
		float q = cube.X, r = cube.Y;
		float w = cellSize.X, h = cellSize.Y;

		Vector2 p = new Vector2(w * 0.75f * q, h * (r + q / 2f));
		return origin + p;
	}

	/**
	 * Inverse of cellToWorld: the cell containing a point in the board's local space.
	 * May return coordinates outside the grid; callers check HexGrid.indexInGrid.
	 *
	 * Exact even for non-regular tiles: the layout is an affine stretch of a regular hex
	 * lattice, so undoing the stretch and cube-rounding picks the correct cell.
	 */
	public Vector2I worldToCell(Vector2 point) {
		Vector2 p = point - origin;
		float q = p.X / (cellSize.X * 0.75f);
		float r = p.Y / cellSize.Y - q / 2f;
		Vector3I cube = HexUtils.cubeRound(new Vector3(q, r, -q - r));
		return HexUtils.cubeToOffset(cube);
	}

	/**
	 * World-space endpoints of the edge a cell shares with its neighbor in `dir`.
	 * Corners start at 0° (east) and step clockwise in screen space (y down), so for
	 * flat-top hexes the edge facing direction d spans corners (d+4)%6 and (d+5)%6
	 * (N = upper-left..upper-right, NE = upper-right..east, and so on).
	 */
	public (Vector2 from, Vector2 to) edgeEndpoints(Vector2I cell, HexDirection dir) {
		Vector2 c = cellToWorld(cell);
		int d = (int)dir;
		return (c + cornerOffsets[(d + 4) % 6], c + cornerOffsets[(d + 5) % 6]);
	}

	/** The tile's bounding box around `center`; tile textures are drawn into this. */
	public Rect2 cellRect(Vector2 center) => new Rect2(center - cellSize / 2f, cellSize);

	/** The six corners of a hex around `center`, optionally shrunk (scale < 1) for insets. */
	public Vector2[] corners(Vector2 center, float scale = 1f) {
		var pts = new Vector2[6];
		for (int i = 0; i < 6; i++) pts[i] = center + cornerOffsets[i] * scale;
		return pts;
	}
	
	public static Rect2 fitRect(Texture2D tex, Vector2 box) {
		if (tex == null) return new Rect2();
		Vector2 size = tex.GetSize();
		float scale = Mathf.Min(box.X / size.X, box.Y / size.Y);
		Vector2 drawn = size * scale;
		return new Rect2(-drawn / 2f, drawn);
	}

}
