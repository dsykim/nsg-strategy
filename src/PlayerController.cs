using Godot;
using System.Diagnostics;

public partial class PlayerController : Node
{
	private UnitController unitController;
	private ResourceController resourceController;
	private CityController cityController;
	public readonly int id;
	private bool alive;

	public PlayerController(int id) {
		this.id = id;
		alive = true;
		Name = "Player" + id;
	}

	public void init() {
		unitController = new UnitController(id);
		resourceController = new ResourceController(id);
		cityController = new CityController(id);
		AddChild(unitController);
		AddChild(resourceController);
		AddChild(cityController);
		unitController.init();
		
		// Connect signals
		unitController.Settle += cityController.handleSettleSignal;
		unitController.UnitCreated += resourceController.handleUnitCreatedSignal;
		cityController.CityCreated += resourceController.handleCityCreatedSignal;

		// TEMP UNIT TEST
		if (id == 0) {
			unitController.createUnit(UnitType.SETTLER, new Vector2I(10, 5));
			unitController.createUnit(UnitType.SETTLER, new Vector2I(11, 6));
		}
	}

	public void turnUpkeep() {
		resourceController.resourceUpkeep();
		unitController.unitUpkeep();
		cityController.cityUpkeep();
	}
}
