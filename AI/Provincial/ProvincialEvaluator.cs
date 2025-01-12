using AI.Evolution;
using AI.Model;
using GameCore;
using GameCore.Cards;

namespace AI.Provincial;

public class ProvincialEvaluator : Evaluator
{
	public override double Evaluate(BuyAgenda agenda, BuyAgenda[] leaders, List<Card> k, int minGames, int maxGames, int parallelDegree)
	{
		double fitness = 0;
		object obj = new object();

		//foreach (var leader in leaders)
		Parallel.ForEach(leaders, new ParallelOptions { MaxDegreeOfParallelism = parallelDegree }, leader =>
		{
			int wins = 0;
			int gameIndex;
			bool significantDifferenceFound = false;
			for (gameIndex = 0; gameIndex < maxGames && !significantDifferenceFound; gameIndex++)
			{
				User[] users = { new ProvincialAI(agenda), new ProvincialAI(leader) };
				Kingdom kingdom = k.GetKingdom(users.Length);

				var game = new Game(users, kingdom);
				var task = game.Play();
				var result = task.Result;

				//wins += result.Score[0].CompareTo(result.Score[1]);
				wins += result.Compare2Players();

				if (gameIndex >= minGames && gameIndex % 200 == 0)
				{
					double errorMargin = 2.0 / Math.Sqrt(gameIndex + 1);
					double spread = Math.Abs(wins / (double)gameIndex);
					significantDifferenceFound = (errorMargin <= spread);
				}
			}

			lock (obj)
			{
				fitness += wins / (double)gameIndex;
			}
			//}
		});
		return fitness;
	}
}