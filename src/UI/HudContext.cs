using System.Linq;

/**
 * Everything a HUD element needs, bundled so UI pieces share one init signature
 * and new ones don't grow every constructor.
 */
public sealed class HudContext
{
	public readonly GameState state;
	public readonly CommandExecutor executor;
	public readonly Selection selection;
	public readonly InputController input;
	public readonly TurnController turns;
	
	public HudContext(GameState state, CommandExecutor executor, Selection selection,
					  InputController input, TurnController turns) {
		this.state = state;
		this.executor = executor;
		this.selection = selection;
		this.input = input;
		this.turns = turns;
	}

	/**
	 * The player whose perspective the HUD shows (resources, etc.).
	 * The current player while a human is playing; during AI turns it stays on the last
	 * human who played, so your gold doesn't flicker to the AI's. Hotseat: switches per human.
	 * With no human players (spectating), it follows the current player.
	 */
	public int viewingPlayer => turns.viewingPlayer;
}
