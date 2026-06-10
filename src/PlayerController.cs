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

		// TEMP UNIT TEST
		if (id == 0) {
			SettlerUnit settler = new SettlerUnit(id);
			MapController.instance.addUnit(settler, new Vector2I(10, 5));
			unitController.addUnit(settler);
		}
	}

	public void turnUpkeep() {
		resourceController.resourceUpkeep();
		unitController.unitUpkeep();
	}
}
