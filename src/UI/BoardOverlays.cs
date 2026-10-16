using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/**
 * View-only outlines drawn over the board: hovered cell, selected cell, and the
 * valid-target region while an action is waiting for a target.
 *
 * Owned by BoardView and laid out with its HexLayout. Knows nothing about input or
 * selection rules; callers just tell it which cells to outline.
 */
public partial class BoardOverlays : Node2D
{
	private HexLayout layout;

	private readonly EdgeOverlay hover = new()
	{
			Name = "Hover",
			LineColor = new Color(1f, 1f, 1f),
			LineWidth = 2f,
	};

	private readonly EdgeOverlay target = new()
	{
			Name = "Target",
			LineWidth = 3f,
	};

	private readonly EdgeOverlay selection = new()
	{
			Name = "Selection",
			LineColor = new Color(1f, 0.85f, 0.3f),
			LineWidth = 3f,
	};

	private readonly EdgeOverlay range = new()
	{
			Name = "Range",
			LineColor = new Color(1f, 0.1f, 0.1f, 0.75f),
			LineWidth = 1.5f,
	};

	private readonly EdgeOverlay p1Borders = new()
	{
		Name = "P1Borders",
		LineColor = new Color(0.3f, 0.3f, 0.9f, 1f),
		LineWidth = 2f,
	};
	
	private readonly EdgeOverlay p2Borders = new()
	{
			Name = "P2Borders",
			LineColor = new Color(0.9f, 0.3f, 0.3f, 1f),
			LineWidth = 2f,
	};

	public BoardOverlays() {
		Name = "Overlays";
		// Child order is draw order: selection sits on top of the target region.
		AddChild(p1Borders);
		AddChild(p2Borders);
		AddChild(range);
		AddChild(hover);
		AddChild(target);
		AddChild(selection);
	}

	/** Called by BoardView whenever the layout is (re)built. */
	public void bind(HexLayout l) {
		layout = l;
		p1Borders.Clear();
		p2Borders.Clear();
		range.Clear();
		hover.Clear();
		target.Clear();
		selection.Clear();
	}

	public void setHover(Vector2I? cell) => outlineSingle(hover, cell);

	public void setSelection(Vector2I? cell) => outlineSingle(selection, cell);

	public void showTargetRegion(IEnumerable<Vector2I> cells, Color color) {
		target.LineColor = color;
		target.SetSegments(regionOutline(cells));
	}

	public void showRangeRegion(IEnumerable<Vector2I> cells) {
		range.SetSegments(regionOutline(cells));
	}

	public void clearTargetRegion() => target.Clear();

	public void clearRangeRegion() => range.Clear();

	public void setBorderRegion(GameState s) {
		var visibleCells = s.getPlayer(TurnController.instance.viewingPlayer).visibleCells;
		var p1Region = s.playerCells(1).Select(c => c.pos).Intersect(visibleCells);
		var p2Region = s.playerCells(2).Select(c => c.pos).Intersect(visibleCells);

		p1Borders.SetSegments(regionOutline(p1Region));
		p2Borders.SetSegments(regionOutline(p2Region));
	}

	/** Edges of `region` that don't border another cell in the region. */
	public List<(Vector2 from, Vector2 to)> regionOutline(IEnumerable<Vector2I> region) {
		var segs = new List<(Vector2, Vector2)>();
		if (layout == null) return segs;

		var inSet = new HashSet<Vector2I>(region);
		foreach (Vector2I pos in inSet)
		foreach (HexDirection d in Enum.GetValues<HexDirection>())
			if (!inSet.Contains(HexUtils.neighbor(pos, d)))
				segs.Add(layout.edgeEndpoints(pos, d));
		return segs;
	}

	private void outlineSingle(EdgeOverlay overlay, Vector2I? cell) {
		if (cell is Vector2I c) overlay.SetSegments(regionOutline(new[] { c }));
		else overlay.Clear();
	}
}
