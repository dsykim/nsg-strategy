
public class PlayerController
{
	private UnitController unitController;
	public readonly int id;
	private bool alive;

	public PlayerController(int id) {
		this.id = id;
		alive = true;
		unitController = new UnitController();
	}
}
