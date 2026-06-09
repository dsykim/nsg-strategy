using System.Collections.Generic;

/**
 * This class manages the units for a single player.
 */
public class UnitController
{
	private List<Unit> units = new List<Unit>();

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
}
