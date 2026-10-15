using Godot;
using System;

public partial class CameraController : Camera2D
{
	/** Bounds on camera translation. */
	private Rect2 bounds;
	/** Translational speed of the camera. */
	private float moveSpeed = 300f;
	/** Change in zoom associated with a single mouse scroll. */
	private float zoomSpeed = 0.1f;
	/** Zoom value to which the camera is currently interpolating. */
	private float targetZoom;
	private float minZoom = 0.5f;
	private float maxZoom = 2f;
	private bool isDragging;

	public void init(Vector2 boardSize) {
		SetPosition(new Vector2());
		targetZoom = Zoom.X;
		bounds = new Rect2(0, 0, boardSize);
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
		clampToBounds();
	}

	private void clampToBounds() {
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

	/* ==================== Drag to pan ==================== */

	private void beginDrag() {
		isDragging = true;
		Input.SetDefaultCursorShape(Input.CursorShape.Drag);
	}

	private void endDrag() {
		if (!isDragging) return;
		isDragging = false;
		Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
	}

	/** Runs before GUI, so drag motion and release are seen even over panels. */
	public override void _Input(InputEvent @event) {
		if (!isDragging) return;

		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Middle && !mb.Pressed) {
			endDrag();
			return;
		}

		if (@event is InputEventMouseMotion mm) {
			// Safety net: if the release was missed, the button mask tells the truth.
			if ((mm.ButtonMask & MouseButtonMask.Middle) == 0) {
				endDrag();
				return;
			}
			// Relative is in screen pixels; divide by zoom to get world units.
			// Subtract so the board moves with the mouse (grab-and-drag feel).
			Position -= mm.Relative / Zoom;
			clampToBounds();
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Middle && mb.Pressed) {
			beginDrag();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed("ZoomIn")) {
			targetZoom += zoomSpeed;
		} else if (@event.IsActionPressed("ZoomOut")) {
			targetZoom -= zoomSpeed;
		}

		targetZoom = Math.Clamp(targetZoom, minZoom, maxZoom);
	}

	/** Alt-tabbing mid-drag would otherwise swallow the release event. */
	public override void _Notification(int what) {
		if (what == NotificationWMWindowFocusOut) endDrag();
	}
}
