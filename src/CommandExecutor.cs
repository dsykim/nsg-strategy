using Godot;

public partial class CommandExecutor : Node
{
	public static CommandExecutor instance { get; private set; }

	public CommandExecutor() {
		instance = this;
		Name = "CommandExecutor";
	}

	public bool submit(Command cmd) {
		if (!cmd.validate()) {
			GD.PrintErr($"Rejected invalid command: {cmd.GetType().Name}");
			return false;
		}
		cmd.execute();
		return true;
	}
}
