using Godot;
using System;

public partial class CameraController : Camera2D
{
	private Rect2 bounds;
	private float moveSpeed = 300f;
	
	private float zoomSpeed = 0.1f;
	private float targetZoom;
	private float minZoom = 0.5f;
	private float maxZoom = 2f;

	public void init() {
		SetPosition(GetViewport().GetVisibleRect().Size/2);
		targetZoom = Zoom.X;
		
		float hexSize = MapController.instance.hexSize;
		Vector2I gridSize = MapController.instance.getGridSize();
		float width =  hexSize * 2 * (((float)3 / 4) * (gridSize.X - 1) + 1);
		float height = hexSize * (float) Math.Sqrt(3) * (gridSize.Y + (float)1 / 2);
		bounds = new Rect2(0, 0, new Vector2(width, height));
	}

	public override void _Process(double delta) {
		handleMovement(delta);
		handleZoom();
	}

	private void handleMovement(double delta) {
		Vector2 movement = new Vector2();
		if (Input.IsActionPressed("PanUp")) {
			// Y is flipped
			movement.Y -= 1;
		}
		if (Input.IsActionPressed("PanRight")) {
			movement.X += 1;
		}
		if (Input.IsActionPressed("PanDown")) {
			movement.Y += 1;
		}
		if (Input.IsActionPressed("PanLeft")) {
			movement.X -= 1;
		}

		float zoomedSpeed = moveSpeed / Zoom.X;
		Position += zoomedSpeed * (float)delta * movement;
		float x = Math.Clamp(Position.X, bounds.Position.X, bounds.End.X);
		float y = Math.Clamp(Position.Y, bounds.Position.Y, bounds.End.Y);
		Position = new Vector2(x, y);
	}

	private void handleZoom() {
		if (Math.Abs(Zoom.X - targetZoom) < 0.001f) {
			Zoom = new Vector2(targetZoom, targetZoom);
		} else {
			float x = float.Lerp(Zoom.X, targetZoom, 0.1f);
			float y = float.Lerp(Zoom.Y, targetZoom, 0.1f);
			Zoom = new Vector2(x, y);
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event.IsAction("ZoomIn")) {
			targetZoom += zoomSpeed;
		} else if (@event.IsAction("ZoomOut")) {
			targetZoom -= zoomSpeed;
		}

		targetZoom = Math.Clamp(targetZoom, minZoom, maxZoom);
	}

}
