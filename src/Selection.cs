using System;

/**
 * What the local human has selected, by entity ID (0 = nothing).
 *
 * Holds an ID rather than a reference so it never points at a stale object, and keeps
 * itself valid: on every StateChanged it drops the selection if the entity is gone
 * (killed, settled into a city, etc.). Create it before other StateChanged listeners so
 * it revalidates first and they never see a dangling ID.
 *
 * Plain C# — no scene graph. Panels, highlights and input all listen to Changed.
 */
public sealed class Selection
{
	private readonly GameState state;

	public int entityID { get; private set; }
	public bool hasSelection => entityID != 0;

	public event Action Changed;

	public Selection(GameState state, CommandExecutor executor) {
		this.state = state;
		executor.StateChanged += revalidate;
	}

	public void select(int id) {
		if (id == entityID) return;
		entityID = id;
		Changed?.Invoke();
	}

	public void clear() => select(0);

	public CellDecorator resolve(GameState s) => entityID == 0 ? null : s.getEntity(entityID);

	private void revalidate() {
		if (entityID != 0 && state.getEntity(entityID) == null) clear();
	}
}
