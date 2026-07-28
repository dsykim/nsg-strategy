using Godot;

public abstract class Command
{
	public int actorID; // player issuing the com
	public int subjectID; // acted-on entity; 0 = none (future player-level actions)

	public abstract bool validate(); // re-check against LIVE state, right before applying

	public abstract void execute();
}

public class MoveCommand : Command
{
	public Vector2I target;

	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectID);
		return u != null && u.owner == actorID && MapController.instance.canMoveUnit(u, target);
	}

	public override void execute() {
		Unit u = EntityRegistry.instance.getUnit(subjectID);
		MapController.instance.moveUnit(u, target);
	}
}

public class AttackCommand : Command
{
	public Vector2I target;

	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectID);
		if (u == null || u.owner != actorID) return false;
		if (u.currentAP < u.attackCost) return false;
		return MapController.instance.getAttackableCells(u).Contains(target);
	}

	public override void execute() {
		Unit u = EntityRegistry.instance.getUnit(subjectID);
		CombatController.instance.resolveCombat(u, target);
	}
}

public class SpawnUnitCommand : Command
{
	public UnitType uType; // subjectID = spawning city

	public override bool validate() {
		City c = EntityRegistry.instance.getCity(subjectID);
		if (c == null || c.owner != actorID) return false;
		PlayerController p = TurnController.instance.getPlayer(actorID);
		return p != null && p.canCreateUnit(uType);
	}

	public override void execute() {
		TurnController.instance.getPlayer(actorID).executeSpawn(subjectID, uType);
	}
}

public class SettleCommand : Command
{
	// subjectID = the settler
	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectID);
		return u is SettlerUnit &&
		       u.owner == actorID &&
		       u.currentAP > 0 &&
		       MapController.instance.canPlaceCity(u.gridPosition);
	}

	public override void execute() {
		TurnController.instance.getPlayer(actorID).executeSettle(subjectID);
	}
}
