public abstract class PlayerDecorator : CellDecorator
{
	public readonly int owner;

	public PlayerDecorator(int owner) {
		this.owner = owner;
	}

	/** Human-readable name for UI and logs. Subclasses override with their own naming. */
	public virtual string displayName => GetType().Name;
}
