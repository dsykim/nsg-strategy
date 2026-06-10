using Godot;

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
			unitController.addUnit(settler);
			MapController.instance.addUnit(settler, new Vector2I(10, 5));
		}
	}

	public void turnUpkeep() {
		resourceController.resourceUpkeep();
		unitController.unitUpkeep();
	}
}
