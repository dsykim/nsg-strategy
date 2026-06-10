using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class InputController : Node
{
    public static InputController instance { get; private set; }

    public enum InputState { Default, SelectingTarget }

    public HexCell hoveredCell { get; private set; } = null;
    public CellDecorator selectedDecorator { get; private set; } = null;
    public Type selectedType;

    private InputState state = InputState.Default;
    private Action<Vector2I> pendingTargetCallback = null;

    [Signal] public delegate void unitSelectedEventHandler(Unit unit);
    [Signal] public delegate void citySelectedEventHandler(City city);

    public void init()
    {
        instance = this;
        Name = "InputController";
    }

    public override void _Process(double delta)
    {
        updateHoveredCell();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent
            && mouseEvent.ButtonIndex == MouseButton.Left
            && mouseEvent.Pressed)
        {
            handleClick();
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                cancelTargetMode();
                return;
            }
            handleKeyAction(keyEvent);
        }
    }

    public void enterSelectTargetMode(Action<Vector2I> callback)
    {
        state = InputState.SelectingTarget;
        pendingTargetCallback = callback;
    }

    public void cancelTargetMode()
    {
        state = InputState.Default;
        pendingTargetCallback = null;
    }

    private void handleClick()
    {
        if (hoveredCell == null) return;

        if (state == InputState.SelectingTarget)
        {
            pendingTargetCallback?.Invoke(hoveredCell.pos);
            pendingTargetCallback = null;
            state = InputState.Default;
            return;
        }

        selectedDecorator = null;
        selectedType = null;
        if (hoveredCell.hasUnit() && selectedDecorator != hoveredCell.units[0])
        {
            // Select unit first unless unit is already selected
            Unit unit = hoveredCell.units[0];
            selectedDecorator = unit;
            selectedType = typeof(Unit);
            EmitSignal(SignalName.unitSelected, unit);
        } else if (hoveredCell.hasCity()) {
            // If no unit or unit already selected, select playerDecorator
            City city = hoveredCell.city;
            selectedDecorator = city;
            selectedType = typeof(City);
            EmitSignal(SignalName.citySelected, city);
        }
    }

    private void handleKeyAction(InputEventKey keyEvent)
    {
        if (selectedDecorator == null) return;

        string key = OS.GetKeycodeString(keyEvent.Keycode);

        if (selectedType == typeof(Unit)) {
            Unit unit = (Unit)selectedDecorator;
            foreach (UnitAction action in unit.actions)
            {
                if (action.keyBinding == key && action.isAvailable)
                {
                    action.onTrigger?.Invoke();
                    return;
                }
            }
        }
    }

    private void updateHoveredCell()
    {
        var camera = GetViewport().GetCamera2D();
        if (camera == null) return;

        var mousePos = camera.GetGlobalMousePosition();
        var spaceState = GetTree().Root.GetWorld2D().DirectSpaceState;

        var query = new PhysicsPointQueryParameters2D();
        query.Position = mousePos;
        query.CollideWithAreas = true;

        Array<Dictionary> results = spaceState.IntersectPoint(query);

        HexCell newHover = null;
        foreach (var result in results)
        {
            var collider = result["collider"].As<Area2D>();
            if (collider?.GetParent() is HexCell cell)
            {
                newHover = cell;
                break;
            }
        }

        hoveredCell = newHover;
    }
}