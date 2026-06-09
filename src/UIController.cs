using Godot;
using Godot.Collections;
using System.Diagnostics;

public partial class UIController : Control
{
	private Label stoneCounter;

	public void init() {
		stoneCounter = GetNode<Label>("StoneCounter");
		
		var resourceController = GetNode<ResourceController>("../TurnController/Player0/ResourceController");
		resourceController.ResourceUpdated += OnResourceUpdated;
	}
	
	private void OnResourceUpdated(Dictionary<string, int> vals)
	{
		stoneCounter.Text = vals["stone"].ToString();
	}
}
