using Godot;

public class CommandExecutor
{
	private GameState state;
	public event System.Action StateChanged;

	public CommandExecutor(GameState s) {
		state = s;
	}

	public bool submit(Command cmd) {
		if (!cmd.validate(state)) {
			GD.PrintErr($"Rejected invalid command: {cmd.GetType().Name}");
			return false;
		}
		
		if (cmd.actorID != state.currentPlayer) {
			GD.PrintErr($"Player {cmd.actorID} submitted command on turn {state.currentPlayer}");
			return false;
		}
		
		cmd.execute(state);
		cmd.print(state);
		StateChanged?.Invoke();
		return true;
	}
}
