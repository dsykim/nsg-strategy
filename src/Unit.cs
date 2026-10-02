using System.Collections.Generic;
using System.Text.Json.Nodes;
using Godot;

public sealed class UnitDef
{
	public UnitType type;
	public string name;
	public int maxHP, maxAP, minRange, maxRange, damage, attackCost, goldCost, capacityCost;
}

/** Static unit data. Load once on the main thread at startup; read-only (and thread-safe) afterwards. */
public static class UnitDefs
{
	private static Dictionary<UnitType, UnitDef> defs;

	public static void load() {
		string json = FileAccess.GetFileAsString("res://src/Units/Units.json");
		var root = JsonNode.Parse(json)!.AsObject();
		defs = new Dictionary<UnitType, UnitDef>();

		foreach (var kvp in root) {
			JsonObject d = kvp.Value!.AsObject();
			var def = new UnitDef
			{
					name = d["name"]!.GetValue<string>(),
					maxHP = d["maxHP"]!.GetValue<int>(),
					maxAP = d["maxAP"]!.GetValue<int>(),
					minRange = d["minRange"]!.GetValue<int>(),
					maxRange = d["maxRange"]!.GetValue<int>(),
					damage = d["damage"]!.GetValue<int>(),
					attackCost = d["attackCost"]!.GetValue<int>(),
					goldCost = d["goldCost"]!.GetValue<int>(),
					capacityCost = d["capacityCost"]!.GetValue<int>(),
			};
			def.type = System.Enum.Parse<UnitType>(def.name, ignoreCase: true);
			defs[def.type] = def;
		}
	}

	public static UnitDef get(UnitType t) => defs[t];
	public static IEnumerable<UnitDef> all() => defs.Values;
}

public abstract class Unit : CellDecorator
{
	public readonly int owner;
	public readonly UnitDef def;

	public int currentHP { get; private set; }
	public int currentAP { get; private set; }

	public UnitType type => def.type;
	public string unitName => def.name;
	public int maxHP => def.maxHP;
	public int maxAP => def.maxAP;
	public int minRange => def.minRange;
	public int maxRange => def.maxRange;
	public int damage => def.damage;
	public int attackCost => def.attackCost;
	public int goldCost => def.goldCost;
	public int capacityCost => def.capacityCost;

	public bool isDead => currentHP <= 0;

	protected Unit(int owner, UnitType type) {
		this.owner = owner;
		def = UnitDefs.get(type);
		currentHP = maxHP;
		currentAP = maxAP;
	}

	public void spendAP(int amount) => currentAP = System.Math.Max(0, currentAP - amount);
	public void restoreAP() => currentAP = maxAP;
	public void applyDamage(int amount) => currentHP = System.Math.Max(0, currentHP - amount);

	public override Unit clone() => (Unit)MemberwiseClone();
}

public class SettlerUnit : Unit { public SettlerUnit(int owner) : base(owner, UnitType.SETTLER) { } }

public class MeleeUnit : Unit { public MeleeUnit(int owner) : base(owner, UnitType.MELEE) { } }

public class RangedUnit : Unit { public RangedUnit(int owner) : base(owner, UnitType.RANGED) { } }

public class SniperUnit : Unit { public SniperUnit(int owner) : base(owner, UnitType.SNIPER) { } }
