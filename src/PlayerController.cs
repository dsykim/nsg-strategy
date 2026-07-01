using Godot;
using System.Diagnostics;
using System.Text.Json.Nodes;

public partial class PlayerController : Node
{
	private UnitController unitController;
	private ResourceController resourceController;
	private CityController cityController;
	public readonly int playerID;
	private bool alive;

	public PlayerController(int playerID) {
		this.playerID = playerID;
		alive = true;
		Name = "Player" + playerID;
	}

	public void init() {
		resourceController = new ResourceController(playerID);
		AddChild(resourceController);
		unitController = new UnitController(playerID);
		AddChild(unitController);
		cityController = new CityController(playerID);
		AddChild(cityController);

		cityController.init();
		unitController.init();

		// Connect signals
		unitController.UnitCreated += resourceController.handleUnitCreatedSignal;
		cityController.CityCreated += resourceController.handleCityCreatedSignal;

		// TEMP UNIT TEST
		if (playerID == 0) {
			unitController.createUnit(UnitType.SETTLER, new Vector2I(10, 5));
			unitController.createUnit(UnitType.SETTLER, new Vector2I(11, 6));
		}
		if (playerID == 1) {
			unitController.createUnit(UnitType.SETTLER, new Vector2I(9, 5));
			unitController.createUnit(UnitType.SETTLER, new Vector2I(12, 6));
		}
	}

	public void turnUpkeep() {
		resourceController.resourceUpkeep();
		unitController.unitUpkeep();
		cityController.cityUpkeep();
	}

	public bool canCreateUnit(UnitType uType) {
		var (goldCost, capacityCost) = UnitController.getUnitCosts(uType);
		return resourceController.canAfford(goldCost) && resourceController.hasCapacityForUnit(capacityCost);
	}

	public void executeSpawn(int cityId, UnitType uType) {
		City c = EntityRegistry.instance.getCity(cityId);
		if (c == null) return;
		unitController.createUnit(uType, c.gridPosition);
	}

	public void executeSettle(int unitId) {
		Unit u = EntityRegistry.instance.getUnit(unitId);
		if (u == null) return;
		Vector2I pos = u.gridPosition;
		unitController.deleteUnit(u);
		cityController.createCity(pos);
		cityController.updateBorders();
	}

	public PlayerSnapshot capturePlayer() => new PlayerSnapshot
	{
			playerID = playerID,
			gold = resourceController.gold,
			goldRate = resourceController.goldRate,
			unitCapacityTotal = resourceController.unitCapacityTotal,
			unitCapacityUsed = resourceController.unitCapacityUsed,
	};
}
