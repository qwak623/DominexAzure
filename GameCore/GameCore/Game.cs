using GameCore.Cards;
using GameCore.GameCore;
using GameCore.Observers;
using Utils;

namespace GameCore;
public class Game
{
	private User[] users;
	public List<Player> Players;
	public Kingdom Kingdom;
	public List<Card> Trash;
	public IGameLogger Logger;

	public bool GameEnd { get; private set; }

	private const int drawCount = 5;
	private GameResults results;

	/// <summary>
	/// </summary>
	/// <param name="users"></param>
	/// <param name="kingdom">Kingdom has to be unique instance for each game.</param>
	/// <param name="logger"></param>
	/// <param name="tokenSource"></param>
	public Game(User[] users, Kingdom kingdom, IGameLogger logger = null, CancellationTokenSource tokenSource = null)
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

	// todo tohle by mělo vracet počáteční info o hráčích, opravit
	public Task<GameResults> Play(int maxRounds = 50)
	{
		return Task.Run(() =>
		{
			// random needs to be instantiated and used in the same thread
			var rnd = new ThreadSafeRandom();

			Players = users.Select(u => new Player(this, u, rnd)).ToList();
			Logger?.Log(new GameLog { Message = "New game has started." });

			// intitial drawing
			Players.ForEach(player => player.Draw(drawCount));

			// player index
			int i = 0, turn = 0;

			// one turn of one player
			while (true)
			{
				Logger?.Log(new GameLog { Message = "\n" });
				if (i == 0)
				{
					Logger?.Log(new GameLog { Message = $"Round {turn}:" });
				}

				Logger?.Log(new GameLog { PlayerId = Players[i].Name, Message = $"{Players[i].Name}'s turn:" });
				Logger?.Log(new GameLog { Message = $"Action phase" });
				Logger?.Log(new GameLog { PlayerId = Players[i].Name, Message = $"Hand: {string.Join(", ", Players[i].ps.Hand.Select(c => c.Name))}" });

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

				Logger?.Log(new GameLog { Message = $"Buy phase" });
				Logger?.Log(new GameLog { PlayerId = Players[i].Name, Message = "Hand: " + string.Join(", ", Players[i].ps.Hand.Select(c => c.Name)) });
				Logger?.Log(new GameLog { PlayerId = Players[i].Name, Message = $"{Players[i].Name} has ${Players[i].ps.Coins}." });

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
					Logger?.Log(new GameLog { Message = "\r\n\tResults:" });
					foreach (Player player in Players.OrderBy(p => p.VictoryPoints))
					{
						Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} has {player.VictoryPoints}." });
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
					Logger?.Log(new GameLog { Message = "\r\nGame was terminated, number of rounds exceeded 50." });
					foreach (Player player in Players.OrderBy(p => p.VictoryPoints))
					{
						Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} has {player.VictoryPoints}." });
					}

					results = new GameResults
					{
						Players = Players,
						Score = new List<int> { 0, 0 },
						Turns = new List<int> { 0, 0 }
					};
				}
			}
		});
	}

	// todo async? nebo ma to tu vubec byt?
	public void RequestPlayerNotifications()
	{
		foreach (var player in Players)
		{
			player.ps.Notify();
		}
	}

	private bool IsGameEnd() => Kingdom.GetPile(CardType.Province).Empty || Kingdom.EmptyPilesCount >= 3;
}
