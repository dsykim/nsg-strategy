using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TargetRequest
{
	public Action<Vector2I> onConfirm;
	public List<HexCell> validCells;
	public Color highlightColor = Colors.White;
}

public partial class UnitController : Node
{
	private List<Unit> units = new List<Unit>();
	private int id;

	public UnitController(int id)
	{
		this.id = id;
		Name = "UnitController";
	}

	public void addUnit(Unit unit)
	{
		if (!units.Contains(unit) && unit != null)
		{
			units.Add(unit);
			initActions(unit);
		}
	}

	public void deleteUnit(Unit unit)
	{
		if (units.Contains(unit))
		{
			units.Remove(unit);
			unit.QueueFree();
		}
	}

	public void unitUpkeep()
	{
		foreach (Unit u in units)
		{
			u.currentAP = u.maxAP;
		}
		checkAvailability();
	}
	
	private void initActions(Unit unit)
	{
		UnitAction moveAction = new UnitAction
		{
				id = "move",
				label = "Move",
				keyBinding = "M",
				isAvailable = false,
				onTrigger = () =>
				{
					InputController.instance.enterSelectTargetMode(new TargetRequest
					{
							validCells = MapController.instance.getMovableCells(unit),
							highlightColor = new Color(1f, 1f, 1f, 1f),
							onConfirm = target =>
							{
								MapController.instance.moveUnit(unit, target);
								checkAvailability();
							}
					});
				}
		};

		unit.addAction(moveAction);
		checkAvailability();
	}

	private void checkAvailability()
	{
		foreach (Unit unit in units)
		{
			bool canMove = unit.currentAP > 0 && hasReachableNeighbor(unit);
			unit.updateAvailability("move", canMove);
		}
	}

	private bool hasReachableNeighbor(Unit unit)
	{
		// Check all 6 neighbors in hex grid offset coordinates
		List<Vector2I> neighbors = MapController.instance.getNeighborPositions(unit.gridPosition);
		return neighbors.Any(pos => MapController.instance.canMoveUnit(unit, pos));
	}
}
