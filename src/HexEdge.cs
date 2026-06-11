using Godot;
using System;

enum EdgeFeature
{
	None,
	Wall,
	River
}

public readonly struct EdgeKey : IEquatable<EdgeKey>
{
	public readonly Vector2I A, B;

	public EdgeKey(Vector2I p, Vector2I q) {
		// normalize so {p,q} and {q,p} collapse to one key
		if (Less(q, p)) {
			A = q;
			B = p;
		} else {
			A = p;
			B = q;
		}
	}

	static bool Less(Vector2I a, Vector2I b) => a.Y != b.Y ? a.Y < b.Y : a.X < b.X;

	public bool Equals(EdgeKey o) => A == o.A && B == o.B;

	public override bool Equals(object o) => o is EdgeKey k && Equals(k);

	public override int GetHashCode() => HashCode.Combine(A, B);
}

public class HexEdge
{
	private EdgeKey key;
	private EdgeFeature feature;

	public HexEdge(EdgeKey key) {
		this.key = key;
	}

}
