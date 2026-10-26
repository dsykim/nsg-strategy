using Godot;

/** Gold, income and unit capacity for the viewing player (see HudContext.viewingPlayer). */
public partial class ResourceBar : HudWidget
{
	[Export] private Label playerLabel;
	[Export] private Label goldLabel;
	[Export] private Label capacityLabel;
	[Export] private Label actionCountLabel;

	protected override void refresh() {
		PlayerState p = ctx.state.getPlayer(ctx.viewingPlayer);
		GameState s = ctx.state;
		if (p == null) return;

		setText(playerLabel, $"Player {p.playerID}");
		setText(goldLabel, $"Gold: {p.gold} (+{p.goldRate})");
		setText(capacityLabel, $"Unit Capacity: {p.unitCapacityUsed} / {p.unitCapacityTotal}");
		setText(actionCountLabel, $"Actions Remaining: {s.MAX_ACTIONS - s.actionCount} / {s.MAX_ACTIONS}");
	}
}
