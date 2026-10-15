using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum HexDirection
{
	N,
	NE,
	SE,
	S,
	SW,
	NW
}

public static class HexUtils
{
	static readonly Vector2I[] evenColOffsets =
	{
			new(0, -1), new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0)
	};
	static readonly Vector2I[] oddColOffsets =
	{
			new(0, -1), new(1, -1), new(1, 0), new(0, 1), new(-1, 0), new(-1, -1)
	};

	public static Vector2I neighbor(Vector2I pos, HexDirection dir) =>
			pos + ((pos.X & 1) == 0 ? evenColOffsets : oddColOffsets)[(int)dir];
	
	public static HexDirection opposite(HexDirection dir) => (HexDirection)(((int)dir + 3) % 6);
	
	private static float lerp(int a, int b, float t) {
		return a + (b - a) * t;
	}
	
	 public static Vector3 cubeLerp(Vector3I c1, Vector3I c2, float t) {
		return new Vector3(
				lerp(c1.X, c2.X, t),
				lerp(c1.Y, c2.Y, t),
				lerp(c1.Z, c2.Z, t)
		);
	}

	public static Vector3I cubeRound(Vector3 coord) {
		return cubeRound(coord.X, coord.Y, coord.Z);
	}

	public static Vector3I cubeRound(double x, double y, double z) {
		int q = (int)Math.Round(x);
		int r = (int)Math.Round(y);
		int s = (int)Math.Round(z);

		double qDiff = Math.Abs(q - x);
		double rDiff = Math.Abs(r - y);
		double sDiff = Math.Abs(s - z);

		if (qDiff > rDiff && qDiff > sDiff) {
			q = -r - s;
		} else if (rDiff > sDiff) {
			r = -q - s;
		} else {
			s = -q - r;
		}
		return new Vector3I(q, r, s);
	}
	
	public static List<(Vector3I A, Vector3I B)> cubeLineSteps(Vector3I c1, Vector3I c2) {
		const double eps = 1e-6;
		int n = hexDistance(c1, c2);
		var steps = new List<(Vector3I, Vector3I)>(n + 1);

		for (int i = 0; i <= n; i++) {
			double t = n == 0 ? 0.0 : (double)i / n;
			double x = c1.X + (c2.X - c1.X) * t;
			double y = c1.Y + (c2.Y - c1.Y) * t;
			double z = c1.Z + (c2.Z - c1.Z) * t;

			Vector3I a = cubeRound(x + eps, y + 2 * eps, z - 3 * eps);
			Vector3I b = cubeRound(x - eps, y - 2 * eps, z + 3 * eps);
			steps.Add((a, b));
		}
		return steps;
	}
	
	public static int hexDistance(Vector3I c1, Vector3I c2) {
		Vector3I vec = c1 - c2;
		return (Math.Abs(vec.X) + Math.Abs(vec.Y) + Math.Abs(vec.Z)) / 2;
	}

	public static int hexDistance(Vector2I cell1, Vector2I cell2) {
		Vector3I c1 = offsetToCube(cell1.X, cell1.Y);
		Vector3I c2 = offsetToCube(cell2.X, cell2.Y);
		return hexDistance(c1, c2);
	}
	public static Vector3I offsetToCube(int x, int y) {
		int q = x;
		int r = y - (x + (x & 1)) / 2;
		return new Vector3I(q, r, -q - r);
	}

	public static Vector2I cubeToOffset(Vector3I c) {
		int col = c.X;
		int row = c.Y + (c.X + (c.X & 1)) / 2;
		return new Vector2I(col, row);
	}
	
	public static List<(Vector2I A, Vector2I B)> getLineSteps(Vector2I p1, Vector2I p2) {
		if (p1 == p2) {
			throw new Exception("Line endpoints must be different");
		}
		Vector3I c1 = offsetToCube(p1.X, p1.Y);
		Vector3I c2 = offsetToCube(p2.X, p2.Y);

		var result = new List<(Vector2I, Vector2I)>();
		foreach (var (a, b) in cubeLineSteps(c1, c2)) {
			result.Add((cubeToOffset(a), cubeToOffset(b)));
		}
		return result;
	}
	
	public static List<Vector2I> getCellsInLine(Vector2I p1, Vector2I p2) {
		return getLineSteps(p1, p2).Select(s => s.A).ToList();
	}
	
	public static bool inLineOfSight(GameState s, Vector2I pos, Vector2I target) {
		if (pos == target) return true;
		var steps = getLineSteps(pos, target);

		// Skip the endpoints: the attacker's and target's own tiles don't block.
		for (int i = 1; i < steps.Count - 1; i++) {
			var (a, b) = steps[i];

			if (a == b) {
				if (MapRules.blocksSight(s, a)) return false;
			} else {
				if (MapRules.blocksSight(s, a) && MapRules.blocksSight(s, b)) return false;
			}
		}
		return true;
	}
	
}
