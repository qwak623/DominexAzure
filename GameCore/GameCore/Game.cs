using GameCore.Cards;
using GameCore.GameCore;
using GameCore.Observers;

namespace GameCore;
public class Game : IGame
{
	private User[] users;
	public List<IPlayer> Players { get; set; }
	public Kingdom Kingdom { get; set; }
	public List<Card> Trash { get; set; }
	public IGameLogger Logger { get; set; }

	public bool GameEnd { get; private set; }

	private const int drawCount = 5;

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
			//var rnd = new ThreadSafeRandom();

			Players = users.Select(u => (IPlayer)new Player(this, u)).ToList();
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

				Players[i].PlayTurn(drawCount);

				GameEnd = IsGameEnd();
				if (GameEnd)
				{
					Logger?.Log(new GameLog { Message = "\r\n\tResults:" });
					foreach (IPlayer player in Players.OrderBy(p => p.VictoryPoints))
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
					GameEnd = true;
					Logger?.Log(new GameLog { Message = "\r\nGame was terminated, number of rounds exceeded {maxRounds}." });
					foreach (IPlayer player in Players.OrderBy(p => p.VictoryPoints))
					{
						Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} has {player.VictoryPoints}." });
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

	// todo async? nebo ma to tu vubec byt?
	public void RequestPlayerNotifications()
	{
		foreach (var player in Players)
		{
			player.PlayerState.Notify();
		}
	}

	private bool IsGameEnd() => Kingdom.GetPile(CardType.Province).Empty || Kingdom.EmptyPilesCount >= 3;
}
