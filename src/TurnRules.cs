using Godot;
using System.Linq;

public static class TurnRules
{
	public static void beginTurn(GameState s, int playerID) {
		PlayerState p = s.getPlayer(playerID);
		p.gold = System.Math.Max(0, p.gold + p.goldRate);
		UnitRules.upkeep(s, playerID);
	}

	public static void endTurn(GameState s) {
		int n = s.players.Count;
		for (int i = 0; i < n; i++) {
			s.currentPlayer = s.currentPlayer % n + 1;
			if (s.currentPlayer == 1) s.turnNumber++;
			if (s.getPlayer(s.currentPlayer).alive) {
				beginTurn(s, s.currentPlayer);
				return;
			}
		}

		if (s.players.Count(p => p.Value.alive) <= 1) {
			GD.Print("Game Over!");
		}
	}
}