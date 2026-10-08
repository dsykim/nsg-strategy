using Godot;

/** Shows the selected unit: shared stats from EntityPanel, plus action points. */
public partial class UnitPanel : EntityPanel
{
	[Export] private Label apLabel;
	[Export] private Range apBar;

	protected override bool shows(CellDecorator e) => e is Unit;

	protected override void refreshStats(CellDecorator e) {
		var u = (Unit)e;
		setText(apLabel, $"{u.currentAP} / {u.maxAP} AP");
		setBar(apBar, u.currentAP, u.maxAP);
	}
}
