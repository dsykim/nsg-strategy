using Godot;
using System.Collections.Generic;
using System.Linq;

/**
 * Something a human can do with a selected entity. Stateless definitions that live in the
 * input/UI layer and only ever produce Commands; the model knows nothing about them.
 *
 * Availability always comes from Command.validate (directly, or on a candidate target), so
 * the UI uses exactly the same rules as the executor and the AI and can't drift from them.
 *
 * New action = new subclass + one line in PlayerActions.build(). Controllers and panels
 * pick it up automatically.
 */
public abstract class PlayerAction
{
	public readonly string id;
	public readonly string label;
	/** InputMap action that triggers this, or null for button-only. */
	public readonly string inputAction;

	protected PlayerAction(string id, string label, string inputAction) {
		this.id = id;
		this.label = label;
		this.inputAction = inputAction;
	}

	/** Whether this action belongs on the entity at all (shown as a button / bound to a key). */
	public abstract bool appliesTo(CellDecorator e);

	/** Whether it can be used right now by `actorID`. Only called when appliesTo(e) is true. */
	public abstract bool isAvailable(GameState s, int actorID, CellDecorator e);
}

/** Fires immediately: settle, spawn. */
public abstract class InstantAction : PlayerAction
{
	protected InstantAction(string id, string label, string inputAction) : base(id, label, inputAction) { }

	public abstract Command build(GameState s, int actorID, CellDecorator e);

	public override bool isAvailable(GameState s, int actorID, CellDecorator e) =>
			build(s, actorID, e).validate(s);
}

/** Needs a target cell: move, attack. Entering it shows the valid region as an outline. */
public abstract class TargetedAction : PlayerAction
{
	public readonly Color highlight;

	protected TargetedAction(string id, string label, string inputAction, Color highlight)
			: base(id, label, inputAction) {
		this.highlight = highlight;
	}

	public abstract IEnumerable<Vector2I> validTargets(GameState s, CellDecorator e);

	public abstract Command build(int actorID, CellDecorator e, Vector2I target);

	/** Available if there's at least one target and a command aimed at it validates. */
	public override bool isAvailable(GameState s, int actorID, CellDecorator e) {
		foreach (Vector2I t in validTargets(s, e)) return build(actorID, e, t).validate(s);
		return false;
	}
}

/* ==================== Unit actions ==================== */

public sealed class MoveAction : TargetedAction
{
	public MoveAction() : base("move", "Move", InputBindings.UnitMove, new Color(0.55f, 0.8f, 1f)) { }

	public override bool appliesTo(CellDecorator e) => e is Unit;

	public override IEnumerable<Vector2I> validTargets(GameState s, CellDecorator e) =>
			MapRules.movableCells(s, (Unit)e).Keys;

	public override Command build(int actorID, CellDecorator e, Vector2I target) =>
			new MoveCommand { actorID = actorID, subjectID = e.id, target = target };
}

public sealed class AttackAction : TargetedAction
{
	public AttackAction() : base("attack", "Attack", InputBindings.UnitAttack, new Color(1f, 0.35f, 0.3f, 0.9f)) { }

	// Units that can't deal damage (settlers) don't get an attack button.
	public override bool appliesTo(CellDecorator e) => e is Unit u && u.damage > 0;

	public override IEnumerable<Vector2I> validTargets(GameState s, CellDecorator e) {
		var u = (Unit)e;
		return u.currentAP < u.attackCost ? Enumerable.Empty<Vector2I>() : UnitRules.getAttackableCells(s, u);
	}

	public override Command build(int actorID, CellDecorator e, Vector2I target) =>
			new AttackCommand { actorID = actorID, subjectID = e.id, target = target };
}

public sealed class SettleAction : InstantAction
{
	public SettleAction() : base("settle", "Settle", InputBindings.UnitSettle) { }

	public override bool appliesTo(CellDecorator e) => e is SettlerUnit;

	public override Command build(GameState s, int actorID, CellDecorator e) =>
			new SettleCommand { actorID = actorID, subjectID = e.id };
}

/* ==================== City actions ==================== */

public sealed class SpawnAction : InstantAction
{
	public readonly UnitDef def;

	public SpawnAction(UnitDef def, string inputAction)
			: base($"spawn_{def.type}", $"{def.name} ({def.goldCost}g)", inputAction) {
		this.def = def;
	}

	public override bool appliesTo(CellDecorator e) => e is City;

	public override Command build(GameState s, int actorID, CellDecorator e) =>
			new SpawnUnitCommand { actorID = actorID, subjectID = e.id, uType = def.type, pos = e.gridPosition };
}

/* ==================== Registry ==================== */

public static class PlayerActions
{
	private static List<PlayerAction> all;

	/** Built on first use (main thread), after UnitDefs.load(). Order is button/display order. */
	public static IReadOnlyList<PlayerAction> All => all ??= build();

	public static IEnumerable<PlayerAction> forEntity(CellDecorator e) =>
			e == null ? Enumerable.Empty<PlayerAction>() : All.Where(a => a.appliesTo(e));

	private static List<PlayerAction> build() {
		var list = new List<PlayerAction>
		{
				new MoveAction(),
				new AttackAction(),
				new SettleAction(),
		};
		
		return list;
	}
}
