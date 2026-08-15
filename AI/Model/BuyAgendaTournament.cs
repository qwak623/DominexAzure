using AI.Provincial;
using GameCore;
using GameCore.Cards;
using GameCore.Observers;

namespace AI.Model;

public static class BuyAgendaTournament
{
	public static void ShowResults(this List<Tuple> agendas, IGameLogger logger)
	{
		foreach (var item in agendas.OrderBy(x => x.Wins))
		{
			logger?.Log(item.ToString());
		}

		logger?.Log("");
	}

	/// <summary>
	/// For each couple of agendas runs specified number of games tvice.
	/// Each couple combination is played so that both players 
	/// start the same number of games.
	/// Games are played in parallel.
	/// </summary>
	/// <param name="agendas"></param>
	/// <param name="k"></param>
	/// <param name="games"></param>
	public static void Tournament(this List<Tuple> agendas, List<Card> k, int games)
	{
		agendas.ForEach(a => a.Wins = 0);

		for (int i = 0; i < agendas.Count; i++)
		{
			for (int j = 0; j < agendas.Count; j++)
			{
				if (i != j)
				{
					Parallel.For(0, games, _ =>
					//for (int c = 0; c < games; c++)
					{
						User[] users = { new ProvincialAI(agendas[i].Agenda), new ProvincialAI(agendas[j].Agenda) };
						Kingdom kingdom = k.GetKingdom(users.Length);

						var game = new Game(users, kingdom);
						var task = game.Play();
						var result = task.Result;

						if (result.PlayerIsWinner(0))
						{
							IncWins(agendas, i);
						}

						if (result.PlayerIsWinner(1))
						{
							IncWins(agendas, j);
						}
					});
					//}
				}

			}
		}
	}

	private static void IncWins(List<Tuple> agendas, int i) => Interlocked.Increment(ref agendas[i].Wins);

	public class Tuple
	{
		public BuyAgenda? Agenda;
		public int Wins;
		public string? Id;

		public override string ToString() => Wins + ": " + Id;
	}
}
