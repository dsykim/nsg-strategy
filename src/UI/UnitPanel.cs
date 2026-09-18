using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class UnitPanel : Control
{
	public static UnitPanel instance { get; private set; }
	[Export] private TextureRect unitImage;
	[Export] private Label nameLabel;
	[Export] private Label hpLabel;
	[Export] private TextureProgressBar hpBar;
	[Export] private Label apLabel;
	[Export] private Container actionButtonContainer;

	private const int playerId = 0;

	private Unit boundUnit;
	private readonly Dictionary<UnitAction, TextureButton> actionButtons = new Dictionary<UnitAction, TextureButton>();

	public void init() {
		instance = this;
		Visible = false;
		InputController.instance.unitSelected += bind;
		InputController.instance.unitDeselected += clear;
	}

	public void bind(Unit unit) {
		if (boundUnit == unit) {
			refreshStats();
			refreshActions();
			return;
		}

		unbindSignals();
		boundUnit = unit;
		unit.statsChanged += refreshStats;
		unit.actionsChanged += refreshActions;

		nameLabel.Text = capitalize(unit.unitName);
		hpLabel.Text = unit.currentHP + "/" + unit.maxHP + " HP";
		apLabel.Text = unit.currentAP + "/" + unit.maxAP + " AP";
 		// unitImage.Texture = unit.Texture;   // swap in when ready

		buildActionButtons();
		refreshStats();
		refreshActions();
		Visible = true;
	}

	public void clear() {
		unbindSignals();
		boundUnit = null;
		Visible = false;
	}

	private void unbindSignals() {
		if (boundUnit != null && GodotObject.IsInstanceValid(boundUnit)) {
			boundUnit.statsChanged -= refreshStats;
			boundUnit.actionsChanged -= refreshActions;
		}
	}

	private void refreshStats() {
		if (boundUnit == null || !GodotObject.IsInstanceValid(boundUnit)) return;
		hpBar.MaxValue = boundUnit.maxHP;
		hpBar.Value = boundUnit.currentHP;
		hpLabel.Text = $"{boundUnit.currentHP} / {boundUnit.maxHP}";
		apLabel.Text = $"{boundUnit.currentAP} / {boundUnit.maxAP}";
	}

	private void buildActionButtons() {
		foreach (Node child in actionButtonContainer.GetChildren()) {
			child.QueueFree();
		}
		actionButtons.Clear();

		if (boundUnit.owner != playerId) return;

		foreach (UnitAction action in boundUnit.actions) {
			UnitAction captured = action;
			TextureButton button = new TextureButton();
			// button.Size = new Vector2(63, 54);
			Texture2D pressed, unpressed;
			switch (action.actionID) {
				case "attack":
					unpressed = ResourceLoader.Load<Texture2D>("res://assets/unitattack_unpressed.png");
					pressed = ResourceLoader.Load<Texture2D>("res://assets/unitattack_pressed.png");
					break;
				case "move":
					unpressed = ResourceLoader.Load<Texture2D>("res://assets/unitmove_unpressed.png");
					pressed = ResourceLoader.Load<Texture2D>("res://assets/unitmove_pressed.png");
					break;
				case "settle":
					unpressed = ResourceLoader.Load<Texture2D>("res://assets/unitsettle_unpressed.png");
					pressed = ResourceLoader.Load<Texture2D>("res://assets/unitsettle_pressed.png");
					break;
				default:
					unpressed = ResourceLoader.Load<Texture2D>("res://assets/unitsettle_unpressed.png");
					pressed = ResourceLoader.Load<Texture2D>("res://assets/unitsettle_pressed.png");
					break;
			}
			
			button.SetTextureNormal(unpressed);
			button.SetTextureHover(pressed);
			button.SetTextureFocused(unpressed);
			button.SetTexturePressed(pressed);
			button.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
			button.CustomMinimumSize = new Vector2(63, 54);
			
			button.Pressed += () => captured.onTrigger?.Invoke();
			actionButtonContainer.AddChild(button);
			actionButtons[action] = button;
		}
	}

	private void refreshActions() {
		if (boundUnit == null) return;
		foreach (var pair in actionButtons) {
			pair.Value.Disabled = !pair.Key.isAvailable;
		}
	}

	private static string capitalize(string s) {
		if (string.IsNullOrEmpty(s)) return s;
		return char.ToUpper(s[0]) + s.Substring(1);
	}
}
