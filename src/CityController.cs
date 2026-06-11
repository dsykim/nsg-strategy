using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class CityController : Node
{
	private List<City> cities;
	private int id;

	public CityController(int id) {
		this.id = id;
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
		mapController.addCity(city);
		cities.Add(city);
		AddChild(city);
	}

	public void cityUpkeep() {
		
	}
}
