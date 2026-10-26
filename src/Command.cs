using Godot;
using System.Threading;

public abstract class Command
{
	public int actorID; // player issuing the com
	public int subjectID; // acted-on entity; 0 = none (future player-level actions)

	// check against state before applying
	public abstract bool validate(GameState state);

	public abstract void execute(GameState state);

	public abstract void print(GameState state);
}

public class MoveCommand : Command
{
	public Vector2I target;

	public override bool validate(GameState state) {
		if (state.actionCount >= state.MAX_ACTIONS) return false;
		Unit u = state.getUnit(subjectID);
		// if (u.hasMoved) return false;
		return u.owner == actorID && u.currentAP > 0 && MapRules.canMove(state, u, target);
	}

	public override void execute(GameState state) {
		Unit u = state.getUnit(subjectID);
		MapRules.moveUnit(state, u, target);
		MapRules.updateVisibility(state);
		state.actionCount++;
	}

	public override void print(GameState state) {
		GD.Print($"Move {subjectID} to {target}");
	}
}

public class AttackCommand : Command
{
	public Vector2I target;

	public override bool validate(GameState state) {
		if (state.actionCount >= state.MAX_ACTIONS) return false;
		Unit u = state.getUnit(subjectID);
		if (u == null || u.owner != actorID || u.hasAttacked) return false;
		return UnitRules.getAttackableCells(state, u).Contains(target);
	}

	public override void execute(GameState state) {
		Unit u = state.getUnit(subjectID);
		CombatRules.resolveCombat(state, u, target);
		MapRules.updateVisibility(state);
		state.actionCount++;
	}
	
	public override void print(GameState state) {
		GD.Print($"{subjectID} attacks {target}");
	}
}

public class SpawnUnitCommand : Command
{
	public UnitType uType;
	public Vector2I pos;

	public override bool validate(GameState state) {
		if (state.actionCount >= state.MAX_ACTIONS) return false;
		City c = state.getCity(subjectID);
		return c != null && c.owner == actorID && pos == c.pos
			   && UnitRules.canCreate(state, actorID, uType, pos);
	}

	public override void execute(GameState state) {
		UnitRules.createUnit(state, actorID, uType, pos);
		MapRules.updateVisibility(state);
		state.actionCount++;
	}
	
	public override void print(GameState state) {
		GD.Print($"Spawned {uType} at {pos}");
	}
}

public class SettleCommand : Command
{
	private Vector2I pos;
	
	public override bool validate(GameState state) {
		if (state.actionCount >= state.MAX_ACTIONS) return false;
		Unit u = state.getUnit(subjectID);
		return u is SettlerUnit &&
			   u.owner == actorID &&
			   MapRules.canPlaceCity(state, u.pos);
	}

	public override void execute(GameState state) {
		Vector2I unitPos = state.getUnit(subjectID).pos;
		pos = unitPos;
		UnitRules.deleteUnit(state, state.getUnit(subjectID));
		CityRules.createCity(state, actorID, pos);
		MapRules.updateVisibility(state);
		state.actionCount++;
	}
	
	public override void print(GameState state) {
		GD.Print($"Settled at {pos}");
	}
}

public class EndTurnCommand : Command
{
	public override bool validate(GameState state) => state.currentPlayer == actorID;
	public override void execute(GameState state) {
		TurnRules.endTurn(state);
		MapRules.updateVisibility(state);
	}
	
	public override void print(GameState state) {
		GD.Print($"Turn ended for player {state.currentPlayer}");
	}
}
