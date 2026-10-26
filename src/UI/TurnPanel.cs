using Godot;

/** Turn number, whose turn it is, and the End Turn button (same path as the EndTurn hotkey). */
public partial class TurnPanel : HudWidget
{
	[Export] private Button endTurnButton;
	[Export] private Label turnLabel;
	[Export] private Label statusLabel;

	public override void _Ready() {
		if (endTurnButton == null) return;
		endTurnButton.FocusMode = FocusModeEnum.None; // Enter is the end-turn hotkey; don't double-fire
		endTurnButton.TooltipText = InputBindings.hint(InputBindings.EndTurn);
		endTurnButton.Pressed += () => ctx?.input.endTurn();
	}

	protected override void refresh() {
		int current = ctx.state.currentPlayer;
		bool myTurn = TurnController.instance.awaitingHuman;

		// turnNumber counts full rounds; +1 assumes setup starts it at 0. Drop it if setup starts at 1.
		setText(turnLabel, $"Turn {ctx.state.turnNumber + 1}");
		setText(statusLabel, myTurn ? $"Your turn player {current}" : $"Player {current} is thinking…");

		if (endTurnButton != null) endTurnButton.Disabled = !myTurn;
	}
}
