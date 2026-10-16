using Godot;
using System.Collections.Generic;

/**
 * The single place the view layer maps model types to textures.
 * Paths are placeholders: point them at your assets.
 *
 * Textures load lazily on first use and are cached. A missing file logs one warning and
 * caches null, so a bad path shows up as an empty cell or sprite rather than a crash.
 * Main thread only (like all view code).
 */
public static class ViewTextures
{
	private static readonly Dictionary<TerrainType, string> terrainPaths = new()
	{
			{ TerrainType.PLAINS, "res://assets/pixel-plains.png" },
			{ TerrainType.HILLS, "res://assets/pixel-hills.png" },
			{ TerrainType.MOUNTAIN, "res://assets/pixel-mountain.png" },
			{ TerrainType.OCEAN, "res://assets/waterHex.png" },
			// EMPTY has no texture and is not drawn.
	};

	private static readonly Dictionary<string, Texture2D> cache = new();

	public static Texture2D terrain(TerrainType t) =>
			terrainPaths.TryGetValue(t, out string path) ? load(path) : null;

	public static Texture2D unit(Unit u) {
		string playerColor = u.owner == 1 ? "blue" : "red";
		string path = "res://assets/" + u.unitName.ToLowerInvariant() + "_" + playerColor + ".png";
		return load(path);
	}

	public static Texture2D city(City c) {
		string playerColor = c.owner == 1 ? "blue" : "red";
		string cityName = c is Capital ? "capital" : "city";
		string path = "res://assets/" + cityName + "_" + playerColor + ".png";
		return load(path);
	}

	private static Texture2D load(string path) {
		if (cache.TryGetValue(path, out Texture2D tex)) return tex;

		tex = ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
		if (tex == null) GD.PushWarning($"Missing texture: {path}");

		cache[path] = tex; // cache misses too, so each bad path warns once
		return tex;
	}
}
