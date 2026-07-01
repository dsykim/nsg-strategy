using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;

public partial class CityController : Node
{
	private List<City> cities = new List<City>();
	private EdgeOverlay borders;
	private ResourceController resourceController;
	private int id;

	[Signal]
	public delegate void CityCreatedEventHandler(City city);

	[Signal]
	public delegate void SpawnButtonClickedEventHandler(string uType, Vector2I spawnPos);

	public CityController(int id) {
		this.id = id;
		Name = "CityController";
		borders = new EdgeOverlay();
		borders.LineColor = new Color(0.9f, 0.3f, 0.3f, 1f);
		AddChild(borders);
	}

	public void init() {
		resourceController = GetNode<ResourceController>("../ResourceController");
	}
	
	public void upgradeCity(City city) {
		
	}

	public void createCity(Vector2I pos) {
		MapController mapController = MapController.instance;
		if (!mapController.canPlaceCity(pos)) {
			Debug.Print("Cannot place city at "+pos);
			return;
		}
		City city = new City(id, pos);
		city.SetPosition(mapController.getCellCenter(pos));
		mapController.addCity(city);
		cities.Add(city);
		AddChild(city);
		initActions(city);
		resourceController.addUnitCapacityTotal(10);
		
		// Assign control of tiles
		updateCityCellControl(city, 1);

		EmitSignal(SignalName.CityCreated, city);
		checkAvailability();
	}

	private void initActions(City city) {
		// For each unit, check if this city is allowed to create it.
		// then init a spawn action for each allowed unit
		// availability of a unit spawn action is based on resource availability 
		string json = FileAccess.GetFileAsString("res://src/Units/Units.json");
		var _data = JsonNode.Parse(json)!.AsObject();
		
		foreach (var kvp in _data)
		{
			// Create a spawn action for each unit
			// TODO: constrain to only add allowed units; add method to add more spawn actions later in game
			JsonObject data = kvp.Value!.AsObject();
			string unitName = data["name"]!.GetValue<string>();
			CityAction spawnUnitAction = new CityAction
			{
					id = "spawn" + unitName,
					label = "Spawn " + unitName,
					isAvailable = false,
					onTrigger = () =>
					{
						EmitSignal(SignalName.SpawnButtonClicked, unitName, city.gridPosition);
						checkAvailability();
					}
			};
			city.addAction(spawnUnitAction);
		}
		
	}
	
	private void checkAvailability() {
		string json = FileAccess.GetFileAsString("res://src/Units/Units.json");
		var _data = JsonNode.Parse(json)!.AsObject();

		foreach (City city in cities) {
			
			foreach (var kvp in _data) {
				// Check spawn action for each unit
				JsonObject data = kvp.Value!.AsObject();
				string unitName = data["name"]!.GetValue<string>();
				bool canCreate = GetParent<PlayerController>().canCreateUnit(data);
				
				string actionID = "spawn" + unitName;
				city.updateAvailability(actionID, canCreate);
			}
		}
	}

	public void updateCityCellControl(City c, int radius) {
		MapController map = MapController.instance;
		var inRangeCells = map.getCellsInRadius(c.gridPosition, radius);
		foreach (var cellPos in inRangeCells) {
			if (map.getCellOwner(cellPos) < 0) {
				c.ownedCells.Add(cellPos);
				map.setCellOwner(cellPos, this.id);
			}
		}
	}
	
	public void handleSettleSignal(SettlerUnit unit) {
		createCity(unit.gridPosition);
		updateBorders();
	}

	public void cityUpkeep() {
		checkAvailability();
	}

	public void updateBorders() {
		var region = new List<Vector2I>();
		foreach (City c in cities) {
			region.AddRange(c.ownedCells);
		}
		var segments = MapController.instance.getRegionOutline(region);
		borders.SetSegments(segments);
	}
}
