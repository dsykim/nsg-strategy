using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class TargetRequest
{
	public Action<Vector2I> onConfirm;
	public List<Vector2I> validCells;
	public Color highlightColor = Colors.White;
}

public enum UnitType
{
	SETTLER
}

public partial class UnitController : Node
{
	private List<Unit> units = new List<Unit>();
	private int totalCapacity;
	private int usedCapacity;
	private int id;

	[Signal] 
	public delegate void SettleEventHandler(SettlerUnit settler);

	[Signal]
	public delegate void UnitCreatedEventHandler(Unit unit);

	public UnitController(int id) {
		this.id = id;
		totalCapacity = 5;
		usedCapacity = 0;
		Name = "UnitController";
	}

	public bool hasCapacityForUnit(Unit unit) {
		// TODO: move constants to JSON so we can access unit values without needing to create the object.
		return unit.capacityCost <= totalCapacity - usedCapacity;
	}

	public void createUnit(UnitType uType, Vector2I pos) {
		MapController mapController = MapController.instance;
		if (!mapController.canPlaceUnit(pos)) {
			Debug.Print("Cannot place unit at " + pos);
			return;
		}
		Unit unit;
		switch (uType) {
			case UnitType.SETTLER:
				unit = new SettlerUnit(id);
				break;
			default:
				unit = new SettlerUnit(id);
				break;
		}

		if (!hasCapacityForUnit(unit)) {
			Debug.Print("No capacity");
			return;
		}
		unit.gridPosition = pos;
		unit.SetPosition(mapController.getCellCenter(pos));
		usedCapacity += unit.capacityCost;
		
		units.Add(unit);
		mapController.addUnit(unit);
		AddChild(unit);
		initActions(unit);

		EmitSignal(SignalName.UnitCreated, unit);
	}

	public void deleteUnit(Unit unit) {
		if (units.Contains(unit)) {
			MapController.instance.removeUnit(unit);
			units.Remove(unit);
			unit.QueueFree();
		}
	}

	public void unitUpkeep() {
		foreach (Unit u in units) {
			u.currentAP = u.maxAP;
		}
		checkAvailability();
	}

	private void initActions(Unit unit) {
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

		
		if (unit.GetType() == typeof(SettlerUnit)) {
			UnitAction settleAction = new UnitAction
			{
					id = "settle",
					label = "Settle",
					keyBinding = "F",
					isAvailable = false,
					onTrigger = () =>
					{
						deleteUnit(unit);
						EmitSignal(SignalName.Settle, unit);
						checkAvailability();
					}
			};
			unit.addAction(settleAction);
		}

		checkAvailability();
	}

	private void checkAvailability() {
		foreach (Unit unit in units) {
			bool canMove = unit.currentAP > 0 && hasReachableNeighbor(unit);
			unit.updateAvailability("move", canMove);

			if (unit.GetType() == typeof(SettlerUnit)) {
				bool canSettle = MapController.instance.canPlaceCity(unit.gridPosition) && unit.currentAP > 0;
				unit.updateAvailability("settle", canSettle);
			}
		}
	}

	private bool hasReachableNeighbor(Unit unit) {
		// Check all 6 neighbors in hex grid offset coordinates
		List<Vector2I> neighbors = MapController.instance.getNeighborPositions(unit.gridPosition);
		return neighbors.Any(pos => MapController.instance.canMoveUnit(unit, pos));
	}
}
