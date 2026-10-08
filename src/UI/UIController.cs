using Godot;

/**
 * Root of the HUD. A plain Node (not a Control) so it can never swallow board clicks;
 * the UICanvas CanvasLayer under it handles screen-space placement.
 *
 * Only job: hand the shared HudContext to each UI element.
 */
public partial class UIController : Node
{
	[Export] private UnitPanel unitPanel;
	[Export] private CityPanel cityPanel;
	[Export] private ResourceBar resourceBar;
	[Export] private TurnPanel turnPanel;

	public void init(HudContext ctx) {
		if (unitPanel != null) unitPanel.init(ctx); else warnMissing(nameof(unitPanel));
		if (cityPanel != null) cityPanel.init(ctx); else warnMissing(nameof(cityPanel));
		if (resourceBar != null) resourceBar.init(ctx); else warnMissing(nameof(resourceBar));
		if (turnPanel != null) turnPanel.init(ctx); else warnMissing(nameof(turnPanel));
	}

	private static void warnMissing(string field) =>
			GD.PushWarning($"UIController: {field} not assigned");
}
