using Godot;
using System.Collections.Generic;

public class WorldSnapshot
{
    public Vector2I gridSize;
    public TerrainTypes[] terrain;
    /** Owner ID for each cell, same ordering as terrain. */
    public int[] cellOwner;

    public Dictionary<int, UnitSnapshot>   units   = new();
    public Dictionary<int, CitySnapshot>   cities  = new();
    public Dictionary<int, PlayerSnapshot> players = new();

    public int          cellIndex(Vector2I p) => p.Y * gridSize.X + p.X;
    public TerrainTypes terrainAt(Vector2I p) => terrain[cellIndex(p)];
    public int          ownerAt(Vector2I p)   => cellOwner[cellIndex(p)];

    public WorldSnapshot clone()
    {
        var c = new WorldSnapshot
        {
            gridSize  = gridSize,
            terrain   = (TerrainTypes[])terrain.Clone(),
            cellOwner = (int[])cellOwner.Clone(),
        };
        foreach (var (id, u)  in units)   c.units[id]   = u.clone();
        foreach (var (id, ct) in cities)  c.cities[id]  = ct.clone();
        foreach (var (id, p)  in players) c.players[id] = p.clone();
        return c;
    }

    public static WorldSnapshot capture()
    {
        var map  = MapController.instance;
        var size = map.getGridSize();
        var snap = new WorldSnapshot
        {
            gridSize  = size,
            terrain   = new TerrainTypes[size.X * size.Y],
            cellOwner = new int[size.X * size.Y],
        };

        for (int y = 0; y < size.Y; y++)
        for (int x = 0; x < size.X; x++)
        {
            var p = new Vector2I(x, y);
            int i = snap.cellIndex(p);
            snap.terrain[i]   = map.getTerrain(p);
            snap.cellOwner[i] = map.getCellOwner(p);
        }

        foreach (Unit u in EntityRegistry.instance.allUnits())
            snap.units[u.id] = UnitSnapshot.from(u);
        foreach (City ct in EntityRegistry.instance.allCities())
            snap.cities[ct.id] = CitySnapshot.from(ct);
        foreach (PlayerController pc in TurnController.instance.allPlayers())
            snap.players[pc.playerID] = pc.capturePlayer();

        return snap;
    }
}

public class UnitSnapshot
{
    public int id, owner;
    public UnitType type;
    public Vector2I pos;
    public int currentHP, maxHP, currentAP, maxAP;
    public int range, damage, attackCost, goldCost, capacityCost;
    
    public UnitSnapshot clone() => (UnitSnapshot)MemberwiseClone();

    public static UnitSnapshot from(Unit u) => new()
    {
        id = u.id, owner = u.owner, type = u.type, pos = u.gridPosition,
        currentHP = u.currentHP, maxHP = u.maxHP,
        currentAP = u.currentAP, maxAP = u.maxAP,
        range = u.range, damage = u.damage, attackCost = u.attackCost,
        goldCost = u.goldCost, capacityCost = u.capacityCost,
    };
}

public class CitySnapshot
{
    public int id, owner;
    public Vector2I pos;
    public int level, goldProduction, currentHP, maxHP;
    public List<Vector2I> ownedCells = new();

    public CitySnapshot clone()
    {
        var c = (CitySnapshot)MemberwiseClone();
        c.ownedCells = new List<Vector2I>(ownedCells);
        return c;
    }

    public static CitySnapshot from(City ct) => new()
    {
        id = ct.id, owner = ct.owner, pos = ct.gridPosition,
        goldProduction = ct.goldProduction,
        currentHP = ct.currentHP, maxHP = ct.maxHP,
        ownedCells = new List<Vector2I>(ct.ownedCells),
    };
}

public class PlayerSnapshot
{
    public int playerID, gold, goldRate, unitCapacityTotal, unitCapacityUsed;
    public PlayerSnapshot clone() => (PlayerSnapshot)MemberwiseClone();
}