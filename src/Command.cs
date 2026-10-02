using Godot;

public abstract class Command
{
	public int actorID; // player issuing the com
	public int subjectID; // acted-on entity; 0 = none (future player-level actions)

	// check against state before applying
	public abstract bool validate(GameState state);

	public abstract void execute(GameState state);
}

public class MoveCommand : Command
{
	public Vector2I target;

	public override bool validate(GameState state) {
		Unit u = state.getUnit(subjectID);
		return u != null && u.owner == actorID && MapRules.canMove(state, u, target);
	}

	public override void execute(GameState state) {
		Unit u = state.getUnit(subjectID);
		MapRules.moveUnit(state, u, target);
	}
}

public class AttackCommand : Command
{
	public Vector2I target;

	public override bool validate(GameState state) {
		Unit u = state.getUnit(subjectID);
		if (u == null || u.owner != actorID) return false;
		if (u.currentAP < u.attackCost) return false;
		return UnitRules.getAttackableCells(state, u).Contains(target);
	}

	public override void execute(GameState state) {
		Unit u = state.getUnit(subjectID);
		CombatRules.resolveCombat(state, u, target);
	}
}

public class SpawnUnitCommand : Command
{
	public UnitType uType;
	public Vector2I pos;

	public override bool validate(GameState state) {
		City c = state.getCity(subjectID);
		return c != null && c.owner == actorID && pos == c.gridPosition
		       && UnitRules.canCreate(state, actorID, uType, pos);
	}

	public override void execute(GameState state) {
		UnitRules.createUnit(state, actorID, uType, pos);
	}
}

public class SettleCommand : Command
{
	public override bool validate(GameState state) {
		Unit u = state.getUnit(subjectID);
		return u is SettlerUnit &&
		       u.owner == actorID &&
		       u.currentAP > 0 &&
		       MapRules.canPlaceCity(state, u.gridPosition);
	}

	public override void execute(GameState state) {
		Vector2I pos = state.getUnit(subjectID).gridPosition;
		UnitRules.deleteUnit(state, state.getUnit(subjectID));
		CityRules.createCity(state, actorID, pos);
	}
}

public class EndTurnCommand : Command
{
	public override bool validate(GameState s) => s.currentPlayer == actorID;
	public override void execute(GameState s) => TurnRules.endTurn(s);
}
