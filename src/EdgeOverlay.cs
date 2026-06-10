using Godot;
using System;
using System.Collections.Generic;

/**
 * A transient drawing layer for hex edge segments — region outlines when a unit
 * is selected, range borders, etc. Holds a flat list of world-space segments and
 * redraws them in _Draw. Computation lives elsewhere (e.g. getRegionOutline);
 * this node only renders whatever segments it's handed.
 */
public partial class EdgeOverlay : Node2D
{
	// Endpoints are world-space pairs as produced by MapController.getEdgeEndpoints.
	private readonly List<(Vector2 from, Vector2 to)> segments = new();

	public Color LineColor { get; set; } = new Color(0.98f, 0.94f, 0.92f, 0.8f);
	public float LineWidth { get; set; } = 3f;
	public bool Antialiased { get; set; } = true;

	// Fills the tiny wedge gaps where two segments meet at a hex vertex. Cheap and
	// makes corners look continuous without ordering the segments into a polyline.
	public bool RoundCaps { get; set; } = true;

	public EdgeOverlay() {
		ZIndex = 20;
	}

	/** Replace the current outline. Pass the output of getRegionOutline(...). */
	public void SetSegments(IEnumerable<(Vector2 from, Vector2 to)> newSegments) {
		segments.Clear();
		segments.AddRange(newSegments);
		QueueRedraw();
	}

	/** Call on deselect to hide the overlay. */
	public void Clear() {
		if (segments.Count == 0) {
			return;
		}
		segments.Clear();
		QueueRedraw();
	}

	public bool HasSegments => segments.Count > 0;

	public override void _Draw() {
		if (segments.Count == 0) {
			return;
		}

		float capRadius = LineWidth / 2f;
		foreach (var (from, to) in segments) {
			DrawLine(from, to, LineColor, LineWidth, Antialiased);

			if (RoundCaps) {
				// Filled dots at both ends close the gaps at shared vertices.
				DrawCircle(from, capRadius, LineColor);
				DrawCircle(to, capRadius, LineColor);
			}
		}
	}
}
