using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using GameCore;
using GameCore.Cards;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Facades.Game;

[Service]
//[Authorize(Roles = nameof(Role.Entry.SystemAdministrator))]
public class GameFacade : IGameFacade
{
	// todo zbavit se static věcí (singleton?)
	private static GameCore.Game Game;

	private static Job<Choice> choice = new();

	private static Job<Answer> answer = new();
	private static State State;
	private static List<Card> cards = new();
	private ClientUser client;
	private static Kingdom kingdom;

	private readonly IGameLogger gameLogger;
	private readonly IKingdomObserver kingdomObserver;
	private readonly IPlayerStateObserver playerStateObserver;

	//private readonly IHubContext<LogHub> logHubContext;

	public GameFacade(IGameLogger gameLogger, IKingdomObserver kingdomObserver, IPlayerStateObserver playerStateObserver)
	{
		this.gameLogger = gameLogger;
		this.kingdomObserver = kingdomObserver;
		this.playerStateObserver = playerStateObserver;

		client = new ClientUser(playerStateObserver);
	}

	public Task Start(CancellationToken cancellationToken = default)
	{
		// todo use cancellation token



		//randomobject = new object();
		//randomStaticObject = new object();

		//je třeba vytvořit usera, tam se budou dít všehchny divnosti
		//var ps = new PlayerState("Šmajdalf");
		if (Game == null)
		{
			List<Card> cards = PresetGames.Get(Games.BigMoney);
			kingdom = cards.GetKingdom(2, kingdomObserver);

			var random = new RandomUser();
			var users = new GameCore.User[] { client, random };

			// todo
			// je třeba vytvořit hru
			// někde ji spustit
			// a vrátit referenci na tu hru? nebo referenci na klienta...
			Game = new GameCore.Game(users, kingdom, gameLogger);
			Game.Play(); // todo continue with results...
		}

		return Task.CompletedTask;

		//return Task.FromResult(choice.Object); // todo vymyslet jak toto může fungovat s async await


		//// todo jb asi bude vhodné toto rozdělit na get a set (možná, asi to nechci mít na jedné stránce)
		//lock (choice)
		//{
		//	choice.Done = false;

		//	while (!choice.Done)
		//	{
		//		Monitor.Wait(choice);
		//	}
		//	//if (tokenSource != null && tokenSource.Token.IsCancellationRequested)
		//	//	throw new OperationCanceledException();
		//	return Task.CompletedTask;

		//}
	}

	public Task<Choice> JoinGame(/*Dto<Guid> gameId, */Dto<int> playerId, CancellationToken cancellationToken = default)
	{
		//lock (choice)
		//{
		//	choice.Done = false;

		//	while (!choice.Done)
		//	{
		//		Monitor.Wait(choice);
		//	}
		//if (tokenSource != null && tokenSource.Token.IsCancellationRequested)
		//	throw new OperationCanceledException();

		return Task.FromResult(choice.Object); // todo vymyslet jak toto může fungovat s async await
		//}
	}

	// todo možná budu chtít jeden interface možná ne
	public Task<Choice> SelectCard(string card, CancellationToken cancellationToken = default)
	{
		lock (answer)
		{
			answer.Object = new Answer
			{
				Card = cards.First(c => c.Name == card)
			};
			answer.Done = true;
			Monitor.Pulse(answer);
		}

		lock (choice)
		{
			choice.Done = false;

			while (!choice.Done)
			{
				Monitor.Wait(choice);
			}
			//if (tokenSource != null && tokenSource.Token.IsCancellationRequested)
			//	throw new OperationCanceledException();
			return Task.FromResult(choice.Object);
		}
	}

	public async Task RequestKingdomNotification(CancellationToken cancellationToken = default)
	{
		if (kingdom is not null)
		{
			await Task.Run(() => kingdomObserver.Notify(kingdom), cancellationToken);
		}
	}

	public async Task RequestPlayerStateNotification(CancellationToken cancellationToken = default)
	{
		await Task.Run(() => Game.RequestPlayerNotifications(), cancellationToken);
	}

	public class ClientUser : GameCore.User
	{
		private readonly IPlayerStateObserver playerStateObserver;

		public ClientUser(IPlayerStateObserver playerStateObserver)
		{
			this.playerStateObserver = playerStateObserver;
		}

		public override IPlayerStateObserver GetPlayerStateObserver()
		{
			return playerStateObserver;
		}

		public override Card BureaucratDiscard(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override List<Card> CellarDiscard(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override bool ChancellorDiscard(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override List<Card> ChapelTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override string GetName() => "Todo Name";

		public override bool LibrarySkip(PlayerState ps, Kingdom k, Card c)
		{
			throw new NotImplementedException();
		}

		public override List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount)
		{
			throw new NotImplementedException();
		}

		public override Card MineTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null)
		{
			throw new NotImplementedException();
		}

		public override Card RemodelTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		{
			lock (choice)
			{
				choice.Object = new Choice
				{
					Cards = wrapper.AvailableCards.Select(c => c.Name).ToList()
				};

				State = new State { };

				choice.Done = true;
				Monitor.Pulse(choice);
			}

			lock (answer)
			{
				answer.Done = false;
				cards = wrapper.AvailableCards.ToList();

				while (!answer.Done)
				{
					Monitor.Wait(answer);
				}
				//if (tokenSource != null && tokenSource.Token.IsCancellationRequested)
				//	throw new OperationCanceledException();
				return answer.Object.Card;
			}
		}

		public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
		{
			throw new NotImplementedException();
		}

		public override Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			throw new NotImplementedException();
		}

		public override bool ThiefSteal(PlayerState ps, Kingdom k, Card c)
		{
			throw new NotImplementedException();
		}

		public override Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			throw new NotImplementedException();
		}
	}

	public class RandomUser : GameCore.User
	{
		public override Card BureaucratDiscard(PlayerState ps, Kingdom k)
		{
			return ps.Hand.FirstOrDefault();
		}

		public override List<Card> CellarDiscard(PlayerState ps, Kingdom k)
		{
			return new();
		}

		public override bool ChancellorDiscard(PlayerState ps, Kingdom k)
		{
			return false;
		}

		public override List<Card> ChapelTrash(PlayerState ps, Kingdom k)
		{
			return new();
		}

		public override string GetName() => "TODO NAME 2";

		public override bool LibrarySkip(PlayerState ps, Kingdom k, Card c)
		{
			return false;
		}

		public override List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount)
		{
			return ps.Hand.Take(2).ToList();
		}

		public override Card MineTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null)
		{
			return null;
		}

		public override Card RemodelTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		{
			return wrapper.AvailableCards.First();
		}

		public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
		{
			throw new NotImplementedException();
		}

		public override Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			throw new NotImplementedException();
		}

		public override bool ThiefSteal(PlayerState ps, Kingdom k, Card c)
		{
			throw new NotImplementedException();
		}

		public override Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			throw new NotImplementedException();
		}
	}

}
