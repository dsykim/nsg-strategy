using Godot;

/**
 * InputMap action names used by the input layer, plus default key bindings.
 *
 * ensureDefaults() only adds an action if it isn't already in the project's Input Map,
 * so anything you define in Project Settings → Input Map wins over these defaults.
 */
public static class InputBindings
{
	public const string EndTurn = "EndTurn";
	public const string UnitMove = "UnitMove";
	public const string UnitAttack = "UnitAttack";
	public const string UnitSettle = "UnitSettle";

	private const int maxSpawnHotkeys = 9;

	/** "CitySpawn1".."CitySpawn9" for the first nine unit types; null beyond that (button-only). */
	public static string spawnAction(int index) =>
			index < maxSpawnHotkeys ? $"CitySpawn{index + 1}" : null;

	/** "Hotkey: M" for tooltips, or "" if the action is unbound. */
	public static string hint(string action) {
		if (action == null || !InputMap.HasAction(action)) return "";
		var events = InputMap.ActionGetEvents(action);
		return events.Count > 0 ? $"Hotkey: {events[0].AsText()}" : "";
	}

	public static void ensureDefaults() {
		ensure(EndTurn, Key.Enter);
		ensure(UnitMove, Key.M);
		ensure(UnitAttack, Key.F);
		ensure(UnitSettle, Key.B);
		for (int i = 0; i < maxSpawnHotkeys; i++)
			ensure(spawnAction(i), (Key)((long)Key.Key1 + i));
	}

	private static void ensure(string action, Key key) {
		if (InputMap.HasAction(action)) return;
		InputMap.AddAction(action);
		InputMap.ActionAddEvent(action, new InputEventKey { Keycode = key });
	}
}
