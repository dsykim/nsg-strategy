using Godot;

/** Placeholder player palette until players carry their own colors. */
public static class PlayerColors
{
	private static readonly Color[] palette =
	{
			new Color("d1495b"), // red
			new Color("3d7dd8"), // blue
			new Color("e9a23b"), // amber
			new Color("5aa469"), // green
			new Color("8e6bbf"), // purple
			new Color("2ab0b0"), // teal
	};

	public static readonly Color neutral = new Color("9a9a9a");

	public static Color get(int playerID) =>
			playerID <= 0 ? neutral : palette[(playerID - 1) % palette.Length];
}
