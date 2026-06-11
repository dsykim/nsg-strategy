public abstract partial class PlayerDecorator : CellDecorator
{
	public readonly int owner;

	public PlayerDecorator(int owner) {
		this.owner = owner;
	}
}
