using Godot;
using System.Collections.Generic;


public partial class UnitController : Node
{
	private List<Unit> units = new List<Unit>();
	private int id;
	
	public UnitController(int id) {
		this.id = id;
		Name = "UnitController";
	}
	
	public void addUnit(Unit unit) {
		if (!units.Contains(unit) && unit != null) {
			units.Add(unit);
		}
	}

	public void deleteUnit(Unit unit) {
		if (units.Contains(unit)) {
			units.Remove(unit);
			unit.QueueFree();
		}
	}

	public void unitUpkeep() {
		foreach (Unit u in units) {
			u.currentAP = u.actionPoints;
		}
	}
}
