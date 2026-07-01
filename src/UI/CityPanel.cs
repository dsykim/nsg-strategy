using Godot;
using System.Collections.Generic;

public partial class CityPanel : VBoxContainer
{
	[Export] private Label nameLabel;
	[Export] private Label hpLabel;
	[Export] private TextureProgressBar hpBar;
	[Export] private Container actionButtonContainer;

	private const int playerId = 0;

	private City boundCity;
	private readonly Dictionary<CityAction, Button> actionButtons = new Dictionary<CityAction, Button>();

	public void init() {
		Visible = false;
		InputController.instance.citySelected += bind;
		InputController.instance.cityDeselected += clear;
	}

	public void bind(City city) {
		if (boundCity == city) {
			refreshStats();
			refreshActions();
			return;
		}

		unbindSignals();
		boundCity = city;
		city.statsChanged += refreshStats;
		city.actionsChanged += refreshActions;

		nameLabel.Text = capitalize(city.cityName);
		// cityImage.Texture = city.Texture;   // swap in when ready

		buildActionButtons();
		refreshStats();
		refreshActions();
		Visible = true;
	}

	public void clear() {
		unbindSignals();
		boundCity = null;
		Visible = false;
	}

	private void unbindSignals() {
		if (boundCity != null && GodotObject.IsInstanceValid(boundCity)) {
			boundCity.statsChanged -= refreshStats;
			boundCity.actionsChanged -= refreshActions;
		}
	}

	private void refreshStats() {
		if (boundCity == null || !GodotObject.IsInstanceValid(boundCity)) return;
		hpBar.MaxValue = boundCity.maxHP;
		hpBar.Value = boundCity.currentHP;
		hpLabel.Text = $"{boundCity.currentHP} / {boundCity.maxHP}";
	}

	private void buildActionButtons() {
		foreach (Node child in actionButtonContainer.GetChildren()) {
			child.QueueFree();
		}
		actionButtons.Clear();

		if (boundCity.owner != playerId) return;

		foreach (CityAction action in boundCity.actions) {
			CityAction captured = action;
			Button button = new Button();
			button.Text = $"{action.label}";
			button.Pressed += () => captured.onTrigger?.Invoke();
			actionButtonContainer.AddChild(button);
			actionButtons[action] = button;
		}
	}

	private void refreshActions() {
		if (boundCity == null) return;
		foreach (var pair in actionButtons) {
			pair.Value.Disabled = !pair.Key.isAvailable;
		}
	}

	private static string capitalize(string s) {
		if (string.IsNullOrEmpty(s)) return s;
		return char.ToUpper(s[0]) + s.Substring(1);
	}
}
