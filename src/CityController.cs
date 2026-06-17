using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class CityController : Node
{
	private List<City> cities = new List<City>();
	private EdgeOverlay borders;
	private int id;

	[Signal]
	public delegate void CityCreatedEventHandler(City city);

	public CityController(int id) {
		this.id = id;
		Name = "CityController";
		borders = new EdgeOverlay();
		borders.LineColor = new Color(0.9f, 0.3f, 0.3f, 1f);
		AddChild(borders);
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
		
		// Assign control of tiles
		updateCityCellControl(city, 1);

		EmitSignal(SignalName.CityCreated, city);
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
