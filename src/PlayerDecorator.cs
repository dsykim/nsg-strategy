public abstract class PlayerDecorator : CellDecorator
{
	public readonly int owner;

	public PlayerDecorator(int owner) {
		this.owner = owner;
	}
}
