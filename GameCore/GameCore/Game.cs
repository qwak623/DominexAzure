using GameCore.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace GameCore;
public class Game
{
	private User[] users;
	public List<Player> Players;
	public Kingdom Kingdom;
	public List<Card> Trash;
	public ILogger Logger;

	public bool GameEnd { get; private set; }

	private const int drawCount = 5;

	/// <summary>
	/// </summary>
	/// <param name="users"></param>
	/// <param name="kingdom">Kingdom has to be unique instance for each game.</param>
	/// <param name="logger"></param>
	/// <param name="tokenSource"></param>
	public Game(User[] users, Kingdom kingdom, ILogger logger = null, CancellationTokenSource tokenSource = null)
	{
		foreach (var user in users)
		{
			user.SetCanCelationTokenSource(tokenSource);
		}

		this.Logger = logger;
		Kingdom = kingdom;
		this.users = users;
		Trash = new List<Card>();
	}

	/// <summary>
	/// Main game loop is implemented here.
	/// Game is calling player methods. 
	/// </summary>
	/// <returns>
	/// Returns Task with results.
	/// </returns>
	public Task<GameResults> Play(int maxRounds = 50)
	{
		return Task.Run(() =>
		{
			// random needs to be instantiated and used in the same thread
			var rnd = new ThreadSafeRandom();

			Players = users.Select(u => new Player(this, u, rnd)).ToList();
			Logger?.Log("New game has started.");

			// intitial drawing
			Players.ForEach(player => player.Draw(drawCount));

			// player index
			int i = 0, turn = 0;

			// one turn of one player
			while (true)
			{
				Logger?.Log("\n");
				if (i == 0)
				{
					Logger?.Log($"Round {turn}:");
				}

				Logger?.Log($"{Players[i].Name}'s turn:");
				Logger?.Log($"Action phase");
				Logger?.Log("Hand: " + string.Join(", ", Players[i].ps.Hand.Select(c => c.Name)));

				Players[i].ps.Buys = 1;
				Players[i].ps.Actions = 1;
				Players[i].ps.Coins = 0;

				// action phase
				Card card;
				do
				{
					card = Players[i].PlayCard();
				}
				while (card != null);

				// treasure phase
				Players[i].PlayTreasure();

				Logger?.Log($"Buy phase");
				Logger?.Log("Hand: " + string.Join(", ", Players[i].ps.Hand.Select(c => c.Name)));
				Logger?.Log($"{Players[i].Name} has ${Players[i].ps.Coins}.");

				// buy phase
				do
				{
					card = Players[i].Buy();
				}
				while (card != null);

				// cleanup phase
				Players[i].Cleanup();

				// draw phase
				Players[i].Draw(drawCount);

				GameEnd = IsGameEnd();
				if (GameEnd)
				{
					Logger?.Log("\r\n\tResults:");
					foreach (Player player in Players.OrderBy(p => p.VictoryPoints))
					{
						Logger?.Log($"{player.Name} has {player.VictoryPoints}.");
					}

					int playerIndex = 0;
					return new GameResults
					{
						Players = Players,
						Score = Players.Select(p => p.VictoryPoints).ToList(),
						Turns = Players.Select(p => playerIndex++ <= i ? turn + 1 : turn).ToList()
					};
				}

				// next player
				i = (i + 1) % Players.Count;

				// stopping too long games
				if (i == 0)
				{
					turn++;
				}

				if (turn >= maxRounds)
				{
					Logger?.Log("\r\nGame was terminated, number of rounds exceeded 50.");
					foreach (Player player in Players.OrderBy(p => p.VictoryPoints))
					{
						Logger?.Log($"{player.Name} has {player.VictoryPoints}.");
					}

					return new GameResults
					{
						Players = Players,
						Score = new List<int> { 0, 0 },
						Turns = new List<int> { 0, 0 }
					};
				}
			}
		});
	}

	private bool IsGameEnd() => Kingdom.GetPile(CardType.Province).Empty || Kingdom.EmptyPiles >= 3;
}
