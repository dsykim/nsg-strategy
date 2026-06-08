using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MapController
{
	private HexGrid hexGrid;
	private float hexSize;
	
	Texture2D oceanTile = ResourceLoader.Load<Texture2D>("res://assets/water_hex.png");
	Texture2D landTile = ResourceLoader.Load<Texture2D>("res://assets/ground_hex.png");

	public MapController(HexGrid hexGrid, float hexSize) {
		this.hexGrid = hexGrid;
		this.hexSize = hexSize;
	}

	public void generateMap() {
		Debug.Print("Generating Map...");

		Queue<HexCell> frontier = new Queue<HexCell>();
		
		HexCell seed = createCell(hexGrid.width / 2, hexGrid.height / 2, TerrainTypes.PLAINS);
		hexGrid.setCell(seed);
		foreach (HexCell c in hexGrid.getNeighbors(seed)) {
			frontier.Enqueue(c);
		}

		Random rand = new Random();
		while (frontier.Count > 0) {
			HexCell next = frontier.Dequeue();
			bool isBorder = next.x == 0 || next.x == hexGrid.width - 1 || next.y == 0 || next.y == hexGrid.height - 1;
			bool isBorderAdj = next.x == 1 || next.x == hexGrid.width - 2 || next.y == 1 || next.y == hexGrid.height - 2;
			int distToSeed = HexGrid.hexDistance(next, seed);
			float threshold = Math.Max(0.95f - (float)Math.Pow((float)distToSeed / hexGrid.width, 2), 0.25f);

			if (isBorderAdj) threshold = 0.2f;
			
			if (!isBorder && rand.NextSingle() < threshold) {
				// Make land
				HexCell generated = createCell(next.x, next.y, TerrainTypes.PLAINS);
				hexGrid.setCell(generated);
				foreach (HexCell c in hexGrid.getNeighbors(generated)) {
					if (!frontier.Contains(c) && c.terrainType == TerrainTypes.EMPTY) {
						frontier.Enqueue(c);
					}
				}
			} else {
				// Make ocean
				HexCell generated = createCell(next.x, next.y, TerrainTypes.OCEAN);
				hexGrid.setCell(generated);
			}
		}

		for (int x = 0; x < hexGrid.width; x++) {
			for (int y = 0; y < hexGrid.height; y++) {
				if (hexGrid.getCell(x, y).terrainType == TerrainTypes.EMPTY) {
					HexCell c = createCell(x, y, TerrainTypes.OCEAN);
					hexGrid.setCell(c);
				}
			}
		}
	}

	private HexCell createCell(int x, int y, TerrainTypes tType) {
		float hexWidth = 2 * hexSize;
		float hexHeight = (float)Math.Sqrt(3) * hexSize;
		HexCell cell = new HexCell(x, y, tType);
		
		Texture2D tex;
		switch (tType) {
			case TerrainTypes.OCEAN:
				tex = oceanTile;
				break;
			case TerrainTypes.PLAINS:
				tex = landTile;
				break;
			default:
				tex = oceanTile;
				break;
		}
		cell.SetTexture(tex);
		cell.SetScale(new Vector2(hexWidth, hexHeight) / tex.GetSize());

		float hOffset = hexWidth / 2f;
		float vOffset = (x & 1) == 0 ? hexHeight : hexHeight / 2f;
		float hStep = 3f / 4 * hexWidth;
		float vStep = (float)Math.Sqrt(3) * hexSize;
		
		cell.SetPosition(new Vector2(hOffset + x*hStep, vOffset + y*vStep));
		return cell;
	}

}
