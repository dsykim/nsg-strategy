using Godot;
using System.Diagnostics;

public partial class PlayerController : Node
{
	private UnitController unitController;
	private ResourceController resourceController;
	public readonly int id;
	private bool alive;

	public PlayerController(int id) {
		this.id = id;
		alive = true;
		unitController = new UnitController(id);
		resourceController = new ResourceController(id);
		AddChild(unitController);
		AddChild(resourceController);
		Name = "Player" + id;
	}

	public void turnUpkeep() {
		resourceController.resourceUpkeep();
		unitController.unitUpkeep();
	}
}
