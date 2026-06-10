using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class UIController : Control
{
	private Label stoneCounter;
	private Sprite2D hexHighlight;
	private HexCell hoveredCell = null;

	public void init() {
		stoneCounter = GetNode<Label>("UICanvas/StoneCounter");
		hexHighlight = GetNode<Sprite2D>("../HexHighlight");
		float hexSize = MapController.instance.hexSize;
		hexHighlight.SetScale(
				new Vector2(2 * hexSize, (float)Math.Sqrt(3) * hexSize) / hexHighlight.Texture.GetSize());
		
		var resourceController = GetNode<ResourceController>("../TurnController/Player0/ResourceController");
		resourceController.ResourceUpdated += OnResourceUpdated;
	}

	public override void _Process(double delta) {
		handleCellHighlight();
	}

	private void handleCellHighlight() {
		var camera = GetViewport().GetCamera2D();
		if (camera == null) return;
		var mousePos = camera.GetGlobalMousePosition();
		var spaceState = GetTree().Root.GetWorld2D().DirectSpaceState;
		
		var query = new PhysicsPointQueryParameters2D();
		query.Position = mousePos;
		query.CollideWithAreas = true;

		var results = spaceState.IntersectPoint(query);
	
		HexCell newHover = null;
		foreach (var result in results)
		{
			var collider = result["collider"].As<Area2D>();
			if (collider?.GetParent() is HexCell cell)
			{
				newHover = cell;
				break;
			}
		}
		if (newHover != hoveredCell && newHover != null) {
			hexHighlight.SetPosition(MapController.instance.getCellCenter(newHover.pos));
			hexHighlight.Visible = true;
			hoveredCell = newHover;
		} else if (newHover == null) {
			hexHighlight.Visible = false;
			hoveredCell = null;
		}
		

	}

	private void OnResourceUpdated(Dictionary<string, int> vals)
	{
		stoneCounter.Text = vals["stone"].ToString();
	}
}
