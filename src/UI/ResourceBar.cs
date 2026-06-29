using Godot;
using Godot.Collections;

public partial class ResourceBar : Control
{
	[Export] private Label goldLabel;
	[Export] private Label capacityLabel;
	
	public void handleResourceUpdateSignal(Dictionary<string, int> vals) {
		goldLabel.Text = vals["gold"].ToString();
		capacityLabel.Text = $"{vals["unitCapacityUsed"]} / {vals["unitCapacityTotal"]}";
	}
}
