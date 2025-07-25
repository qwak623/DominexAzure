using System.Collections.Generic;
using System.Linq;

namespace GameCore;
public class GameResults
{
	public List<int> Score { get; set; }
	public List<IPlayer> Players;
	public List<int> Turns;

	/// <summary>
	/// Indicates if player with index is winner or not
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public bool PlayerIsWinner(int index)
	{
		int maxScore = Score.Max();
		if (Score[index] != maxScore)
			return false;
		if (Score.Where(s => s == maxScore).Count() == 1)
			return true;

		int turnChangeIndex = 0;
		int previous = Turns[0];
		for (int i = 0; i < Turns.Count; i++)
		{
			if (previous != Turns[i])
			{
				turnChangeIndex = i;
				break;
			}
		}

		return index >= turnChangeIndex || !Score.Skip(turnChangeIndex).Where(s => s == maxScore).Any();
	}

	/// <summary>
	/// Faster way to indicate winner, when there are only two players.
	/// </summary>
	/// <returns></returns>
	public int Compare2Players()
	{
		int cmp = Score[0].CompareTo(Score[1]);
		return cmp != 0 ? cmp : -Turns[0].CompareTo(Turns[1]);
	}
}