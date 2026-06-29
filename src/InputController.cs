using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class InputController : Node
{
	public static InputController instance { get; private set; }

	public enum InputState
	{
		Default,
		SelectingTarget,
		SelectingCityAction
	}

	private InputState state = InputState.Default;

	public HexCell hoveredCell { get; private set; } = null;
	public CellDecorator selectedDecorator { get; private set; } = null;
	public Type selectedType;

	private Action<Vector2I> pendingTargetCallback = null;
	private TargetRequest pendingRequest = null;
	private HashSet<Vector2I> validTargets = null;

	[Signal] public delegate void unitSelectedEventHandler(Unit unit);
	[Signal] public delegate void unitDeselectedEventHandler();
	[Signal] public delegate void citySelectedEventHandler(City city);

	[Signal]
	public delegate void cityDeselectedEventHandler();
	

	public void init() {
		instance = this;
		Name = "InputController";
	}

	public override void _Process(double delta) {
		updateHoveredCell();
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.Pressed) {
			handleClick();
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo) {
			if (keyEvent.Keycode == Key.Escape) {
				cancelTargetMode();
				return;
			}
			handleKeyAction(keyEvent);
		}
	}

	public void enterSelectTargetMode(TargetRequest request) {
		state = InputState.SelectingTarget;
		pendingRequest = request;
		validTargets = new HashSet<Vector2I>(request.validCells.Select(c => c));
		MapController.instance.showTargetRegion(request.validCells, request.highlightColor);
	}

	public void cancelTargetMode() {
		state = InputState.Default;
		pendingRequest = null;
		validTargets = null;
		MapController.instance.clearTargetRegion();
		EmitSignal(SignalName.cityDeselected);
	}

	public void enterCityActionSelectMode() {
		state = InputState.SelectingCityAction;
	}

	private void handleClick() {
		if (hoveredCell == null) return;

		if (state == InputState.SelectingTarget) {
			if (validTargets != null && !validTargets.Contains(hoveredCell.pos)) {
				cancelTargetMode();
				return;
			}

			var request = pendingRequest;
			cancelTargetMode();
			request.onConfirm?.Invoke(hoveredCell.pos);
			return;
		}
		
		if (hoveredCell.hasUnit() && selectedDecorator != hoveredCell.units[0]) {
			// Select unit first unless unit is already selected
			EmitSignal(SignalName.cityDeselected);
			Unit unit = hoveredCell.units[0];
			selectedDecorator = unit;
			selectedType = typeof(Unit);
			EmitSignal(SignalName.unitSelected, unit);
		} else if (hoveredCell.hasCity()) {
			// If no unit or unit already selected, select playerDecorator
			EmitSignal(SignalName.unitDeselected);
			City city = hoveredCell.city;
			selectedDecorator = city;
			selectedType = typeof(City);
			enterCityActionSelectMode();
			EmitSignal(SignalName.citySelected, city);
			
		} else {
			// Empty select, deselect
			if (selectedType == typeof(Unit)) {
				EmitSignal(SignalName.unitDeselected);
			} else if (selectedType == typeof(City)) {
				EmitSignal(SignalName.cityDeselected);
			}
			selectedDecorator = null;
			selectedType = null;
		}
	}

	private void handleKeyAction(InputEventKey keyEvent) {
		if (selectedDecorator == null) return;

		string key = OS.GetKeycodeString(keyEvent.Keycode);

		if (selectedType == typeof(Unit)) {
			Unit unit = (Unit)selectedDecorator;
			foreach (UnitAction action in unit.actions) {
				if (action.keyBinding == key && action.isAvailable) {
					action.onTrigger?.Invoke();
					return;
				}
			}
		}
	}

	private void updateHoveredCell() {
		var camera = GetViewport().GetCamera2D();
		if (camera == null) return;

		var mousePos = camera.GetGlobalMousePosition();
		var spaceState = GetTree().Root.GetWorld2D().DirectSpaceState;

		var query = new PhysicsPointQueryParameters2D();
		query.Position = mousePos;
		query.CollideWithAreas = true;

		Array<Dictionary> results = spaceState.IntersectPoint(query);

		HexCell newHover = null;
		foreach (var result in results) {
			var collider = result["collider"].As<Area2D>();
			if (collider?.GetParent() is HexCell cell) {
				newHover = cell;
				break;
			}
		}

		hoveredCell = newHover;
	}
}
