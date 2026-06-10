using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class UIController : Control
{
	private Label stoneCounter;
	private Sprite2D hexHighlight;
	private Sprite2D hexSelect;

	public void init()
	{
		stoneCounter = GetNode<Label>("UICanvas/StoneCounter");
		hexHighlight = GetNode<Sprite2D>("../HexHighlight");
		hexSelect = GetNode<Sprite2D>("../HexSelect");
		float hexSize = MapController.instance.hexSize;
		Vector2 hexScale = new Vector2(2 * hexSize, (float)Math.Sqrt(3) * hexSize) / hexHighlight.Texture.GetSize();
		hexHighlight.SetScale(hexScale);
		hexSelect.SetScale(hexScale);
		hexHighlight.Visible = false;
		hexSelect.Visible = false;

		var resourceController = GetNode<ResourceController>("../TurnController/Player0/ResourceController");
		resourceController.ResourceUpdated += onResourceUpdated;
	}

	public override void _Process(double delta)
	{
		handleCellHighlight();
		handleCellSelect();
	}

	private void handleCellHighlight()
	{
		HexCell hovered = InputController.instance.hoveredCell;

		if (hovered != null)
		{
			hexHighlight.SetPosition(MapController.instance.getCellCenter(hovered.pos));
			hexHighlight.Visible = true;
		}
		else
		{
			hexHighlight.Visible = false;
		}
	}

	private void handleCellSelect() {
		CellDecorator selected = InputController.instance.selectedDecorator;

		if (selected != null) {
			hexSelect.Visible = true;
			hexSelect.SetPosition(MapController.instance.getCellCenter(selected.gridPosition));
		} else {
			hexSelect.Visible = false;
		}
	}

	private void onResourceUpdated(Dictionary<string, int> vals)
	{
		stoneCounter.Text = vals["stone"].ToString();
	}
}
