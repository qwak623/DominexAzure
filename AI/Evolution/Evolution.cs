using AI.Model;
using AI.Provincial;
using GameCore;
using GameCore.Observers;
using Utils;

namespace AI.Evolution;

public class Evolution
{
	private Params par;
	private BuyAgenda[] leaders;
	private (BuyAgenda Agenda, double Fitness)[] pool;
	private BuyAgenda referenceAgenda;
	private readonly IGameLogger logger;

	/// <summary>
	/// </summary>
	/// <param name="par"></param>
	/// <param name="logger"></param>
	/// <param name="referenceAgenda">
	///     If reference agenda is not null, best idividual will be confronted with referenceAgenda after each evaluation
	///     and the results will be logged using logger.
	/// </param>
	public Evolution(Params par, IGameLogger? logger = null, BuyAgenda? referenceAgenda = null)
	{
		this.par = par;
		this.logger = logger;
		this.referenceAgenda = referenceAgenda;
	}

	/// <summary>
	/// </summary>
	/// <returns> 
	/// Returns evolved BuyAgenda. This individual is not saved in this method.
	/// </returns>
	public BuyAgenda Run()
	{
		SetUp();

		var sw = new System.Diagnostics.Stopwatch();

		for (int gen = 0; gen < par.Generations; gen++)
		{
			sw.Reset();
			sw.Start();

			// evolution step
			GenerateNewPool();
			Evaluate();
			SetNewLeaders();

			sw.Stop();
			var elapsed = sw.Elapsed;

			if (referenceAgenda is not null)
			{
				ComputeFitness(leaders[0], gen);
			}
			//referenceAgenda = leaders[0];

			logger?.Log($"Generation {gen}: elapsed time {elapsed.TotalSeconds.ToString("0.00")}s");
		}

		return leaders[0];
	}

	public void SetUp()
	{
		leaders = new BuyAgenda[par.LeaderCount];
		for (int i = 0; i < leaders.Length; i++)
		{
			leaders[i] = BuyAgenda.CreateRandom(par.Kingdom.AddRequiredCards());
		}

		BuyAgenda.CreateRandom(par.Kingdom.AddRequiredCards());
		pool = new (BuyAgenda, double)[par.PoolCount];
	}

	private void Evaluate()
	{
		//for (int i = 0; i < pool.Length; i++)
		Parallel.For(0, pool.Length, new ParallelOptions { MaxDegreeOfParallelism = par.ParallelDegreeExt }, i =>
			pool[i].Fitness = par.Evaluator.Evaluate(pool[i].Agenda, leaders, par.Kingdom, par.MinGames, par.MaxGames, par.ParallelDegreeInt));
	}

	/// <summary>
	/// Trying to find best but not resebling agendas for new leaders using levenstein distance.
	/// </summary>
	private void SetNewLeaders()
	{
		// comparing fitness and individual length
		Array.Sort(pool, (a, b) => (-2 * a.Fitness.CompareTo(b.Fitness)) + a.Agenda.BuyMenu.Count.CompareTo(b.Agenda.BuyMenu.Count));
		int j = 0;
		for (int i = 0; i < leaders.Length;)
		{
			if (IsSimilarToAny(pool[j].Agenda, i))
			{
				if (j > pool.Length)
				{
					leaders[i++] = BuyAgenda.CreateRandom(par.Kingdom.AddRequiredCards());
				}
				else
				{
					j++;
				}

				continue;
			}
			leaders[i++] = pool[j++].Agenda;
		}
	}

	/// <summary>
	/// Leaders are added also to pool unchanged.
	/// Rest is filled with mutated leaders.
	/// </summary>
	private void GenerateNewPool()
	{
		// first leadersSize members without changes
		for (int i = 0; i < leaders.Length; i++)
		{
			pool[i] = (leaders[i], 0);
		}

		// for each new member in pool
		for (int i = leaders.Length; i < pool.Length; i++)
		{
			var agenda = leaders[ThreadSafeRandom.Next(leaders.Length)].Clone();

			// at least one mutations always happens
			do
			{
				par.MutationSelector.SelectMutation(agenda.BuyMenu.Count).Mutate(agenda, par.Kingdom.AddRequiredCards());
			}
			while (ThreadSafeRandom.NextDouble() < par.Mutate);

			pool[i] = (agenda, 0);
		}
	}

	/// <summary>
	/// This method compares the leader and referencer.
	/// </summary>
	/// <param name="buyAgenda"></param>
	/// <param name="generation"></param>
	private void ComputeFitness(BuyAgenda buyAgenda, int generation)
	{
		User getFirst() => new ProvincialAI(referenceAgenda, "Referencer");
		User getSecond() => new ProvincialAI(buyAgenda, "Leader");
		const int gameCount = 5000;

		int[] result = new int[3];
		for (int i = 0; i < gameCount; i++)
		{
			IGame game = new Game(new User[] { getFirst(), getSecond() }, par.Kingdom.GetKingdom(2));
			var task = game.Play();
			var results = task.Result;
			if (results.PlayerIsWinner(0))
			{
				result[0]++;
			}

			if (results.PlayerIsWinner(1))
			{
				result[1]++;
			}

			result[2] -= results.Compare2Players();
		}

		for (int i = 0; i < gameCount; i++)
		{
			IGame game = new Game(new User[] { getSecond(), getFirst() }, par.Kingdom.GetKingdom(2));
			var task = game.Play();
			var results = task.Result;
			if (results.PlayerIsWinner(0))
			{
				result[1]++;
			}

			if (results.PlayerIsWinner(1))
			{
				result[0]++;
			}

			result[2] += results.Compare2Players();
		}

		//if (result[1] > result[0])
		//   buyAgenda.Save(par.Kingdom, $"gen_{generation}({result[0]}, {result[1]})");
		//logger?.Log($"{generation}: Referencer {result[0]}, Leader {result[1]}, Ratio {result[2]}");
		logger?.Log($"{generation} {result[0]} {result[1]} {result[2]}");

	}

	private bool IsSimilarToAny(BuyAgenda agenda, int count)
	{
		int aggDistance = 0;
		for (int i = 0; i < count; i++)
		{
			aggDistance += agenda.BuyMenu.CalcLevensteinDistance(leaders[i].BuyMenu);
		}

		if (aggDistance > (count * 1.5f) - 1)
		{
			return false;
		}

		return true;
	}
}