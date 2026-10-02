using Godot;
using System;
using System.Collections.Generic;

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
		int q = (int) Math.Round(coord.X);
		int r = (int)Math.Round(coord.Y);
		int s = (int)Math.Round(coord.Z);

		float qDiff = Math.Abs(q - coord.X);
		float rDiff = Math.Abs(r - coord.Y);
		float sDiff = Math.Abs(s - coord.Z);

		if (qDiff > rDiff && qDiff > sDiff) {
			q = -r - s;
		} else if (rDiff > sDiff) {
			r = -q - s;
		} else {
			s = -q - r;
		}

		return new Vector3I(q, r, s);
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

	
}
