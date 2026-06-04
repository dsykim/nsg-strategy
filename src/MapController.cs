using Godot;
using System;
using System.Diagnostics;

public class MapController
{
	private HexGrid hexGrid;
	private float hexSize;

	public MapController(HexGrid hexGrid, float hexSize) {
		this.hexGrid = hexGrid;
		this.hexSize = hexSize;
	}

	public void generateMap() {
		Debug.Print("Generating Map...");
		Texture2D oceanTile = ResourceLoader.Load<Texture2D>("res://assets/water_hex.png");
		Texture2D landTile = ResourceLoader.Load<Texture2D>("res://assets/ground_hex.png");
		float hexWidth = 2 * hexSize;
		float hexHeight = (float)Math.Sqrt(3) * hexSize;

		for (int x = 0; x < hexGrid.width; x++) {
			for (int y = 0; y < hexGrid.height; y++) {
				// Generate checkerboard for now
				Texture2D tex = (x + y) % 2 == 0 ? oceanTile : landTile;
				TerrainTypes tType = (x + y) % 2 == 0 ? TerrainTypes.OCEAN : TerrainTypes.PLAINS;
				
				HexCell cell = new HexCell(x, y, tType);
				cell.SetTexture(tex);
				cell.SetScale(new Vector2(hexWidth, hexHeight) / tex.GetSize());

				float hOffset = hexWidth / 2f;
				float vOffset = (x & 1) == 0 ? hexHeight : hexHeight / 2f;
				float hStep = 3f / 4 * hexWidth;
				float vStep = (float)Math.Sqrt(3) * hexSize;
				
				cell.SetPosition(new Vector2(hOffset + x*hStep, vOffset + y*vStep));
				
				hexGrid.setCell(cell);
			}
		}
	}

}
