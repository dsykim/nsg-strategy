using Godot;

/** Which board layer a view is drawn on. Declaration order is draw order (later = on top). */
public enum ViewLayer
{
	Cities,
	Units,
}

/**
 * Base for anything drawn for a model entity.
 *
 * Holding the ID rather than the object keeps a view valid when the whole GameState is replaced
 * (e.g. on load), since IDs are save-stable but the objects are not.
 */
public abstract partial class EntityView : Node2D
{
	public int entityID { get; private set; }

	public abstract ViewLayer layer { get; }

	public void bindEntity(int id) {
		entityID = id;
		Name = $"{GetType().Name}_{id}";
	}

	/** Read the entity from state and update visuals. Called by BoardView only. */
	public abstract void sync(GameState s, HexLayout layout);
}

/** Maps a model entity to a new view. New entity kinds add a case here and nowhere else. */
public static class EntityViewFactory
{
	public static EntityView create(CellDecorator e) => e switch
	{
			Unit => new UnitView(),
			City => new CityView(),
			_ => null,
	};
}
