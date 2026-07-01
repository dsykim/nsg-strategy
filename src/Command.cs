using Godot;

public abstract class Command
{
	public int actorId; // player issuing the command
	public int subjectId; // acted-on entity; 0 = none (future player-level actions)

	public abstract bool validate(); // re-check against LIVE state, right before applying

	public abstract void execute();
}

public class MoveCommand : Command
{
	public Vector2I target;

	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectId);
		return u != null && u.owner == actorId && MapController.instance.canMoveUnit(u, target);
	}

	public override void execute() {
		Unit u = EntityRegistry.instance.getUnit(subjectId);
		MapController.instance.moveUnit(u, target);
	}
}

public class AttackCommand : Command
{
	public Vector2I target;

	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectId);
		if (u == null || u.owner != actorId) return false;
		if (u.currentAP < u.attackCost) return false;
		return MapController.instance.getAttackableCells(u).Contains(target);
	}

	public override void execute() {
		Unit u = EntityRegistry.instance.getUnit(subjectId);
		CombatController.instance.resolveCombat(u, target);
	}
}

public class SpawnUnitCommand : Command
{
	public UnitType uType; // subjectId = spawning city

	public override bool validate() {
		City c = EntityRegistry.instance.getCity(subjectId);
		if (c == null || c.owner != actorId) return false;
		PlayerController p = TurnController.instance.getPlayer(actorId);
		return p != null && p.canCreateUnit(uType);
	}

	public override void execute() {
		TurnController.instance.getPlayer(actorId).executeSpawn(subjectId, uType);
	}
}

public class SettleCommand : Command
{
	// subjectId = the settler
	public override bool validate() {
		Unit u = EntityRegistry.instance.getUnit(subjectId);
		return u is SettlerUnit &&
		       u.owner == actorId &&
		       u.currentAP > 0 &&
		       MapController.instance.canPlaceCity(u.gridPosition);
	}

	public override void execute() {
		TurnController.instance.getPlayer(actorId).executeSettle(subjectId);
	}
}
