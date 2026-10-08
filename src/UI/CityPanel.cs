using Godot;

/** Shows the selected city: shared stats from EntityPanel, plus income. */
public partial class CityPanel : EntityPanel
{
	[Export] private Label incomeLabel;

	protected override bool shows(CellDecorator e) => e is City;

	protected override void refreshStats(CellDecorator e) {
		var c = (City)e;
		setText(incomeLabel, $"+{c.goldProduction} gold / turn");
	}
}
