public class NaturalDecorator : CellDecorator
{
	public override CellDecorator clone() => (NaturalDecorator) MemberwiseClone();
}
