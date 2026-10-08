/** Anything that can take damage. Lets UI and debug output treat units and cities alike. */
public interface IHasHealth
{
	int currentHP { get; }
	int maxHP { get; }
}
