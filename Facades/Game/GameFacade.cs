using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Dominex.Services.Game;
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

	private static Job<Choice> choiceJob = new();

	private static Job<Answer> answerJob = new();
	private static State State;
	private static List<Card> cards = new();
	private ClientUser client;
	private static Kingdom kingdom;

	private readonly IGameLogger gameLogger;
	private readonly IKingdomObserver kingdomObserver;
	private readonly IPlayerStateObserver playerStateObserver;

	//private readonly IHubContext<LogHub> logHubContext;

	public GameFacade(
		IGameLogger gameLogger,
		IKingdomObserver kingdomObserver,
		IPlayerStateObserver playerStateObserver)
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

		return Task.FromResult(choiceJob.Object); // todo vymyslet jak toto může fungovat s async await
	}

	// todo možná budu chtít jeden interface možná ne
	public Task<Choice> Submit(Answer answer, CancellationToken cancellationToken = default)
	{
		lock (answerJob)
		{
			answerJob.Object = answer; // todo tohle je hloupe pojmenování
			answerJob.Done = true;
			Monitor.Pulse(answerJob);
		}

		lock (choiceJob)
		{
			choiceJob.Done = false;

			while (!choiceJob.Done)
			{
				Monitor.Wait(choiceJob);
			}
			//if (tokenSource != null && tokenSource.Token.IsCancellationRequested)
			//	throw new OperationCanceledException();
			return Task.FromResult(choiceJob.Object);
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
		private CardMapper cardMapper = new(); // todo tohle je hnus

		public ClientUser(IPlayerStateObserver playerStateObserver)
		{
			this.playerStateObserver = playerStateObserver;
		}

		public override IPlayerStateObserver GetPlayerStateObserver()
		{
			return playerStateObserver;
		}

		public T LockTemplate<T>(Choice choice, Func<Answer, T> answerFunc)
		{
			lock (choiceJob)
			{
				choiceJob.Object = choice;
				choiceJob.Done = true;
				Monitor.Pulse(choiceJob);
			}

			lock (answerJob)
			{
				answerJob.Done = false;
				while (!answerJob.Done)
				{
					Monitor.Wait(answerJob);
				}

				// todo chtělo by to tady udělat kontroly range
				if (answerJob.Object.Values.Count < choice.MinNumberOfSelections)
				{
					throw new ArgumentOutOfRangeException($"Minimal count of cards with non-default operation is {choice.MinNumberOfSelections} but actual number is {answerJob.Object.Values.Count}.");
				}
				else if (answerJob.Object.Values.Count > choice.MaxNumberOfSelections)
				{
					throw new ArgumentOutOfRangeException($"Maximal  count of cards with non-default operation is {choice.MaxNumberOfSelections} but actual number is {answerJob.Object.Values.Count}.");
				}

				foreach (var value in answerJob.Object.Values)
				{
					var correspondingChoice = choice.Values.SingleOrDefault(c => c.Card.Name == value.Card.Name)
						?? throw new ArgumentException($"Chosen card {value.Card.Name} was not present in the options.");

					if (!correspondingChoice.Operations.Contains(value.OperationType))
					{
						throw new ArgumentException($"Chosen operation type {value.OperationType} for card {value.Card.Name} was not present in the options for this card.");
					}
				}

				return answerFunc(answerJob.Object);
			}
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
			return LockTemplate(new Choice
			(
				ChoiceType.Play,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: cards.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Play }
			), answer =>
			{
				string cardName = answer.Values.SingleOrDefault(c => c.OperationType == OperationType.Play)?.Card?.Name;
				return cards.SingleOrDefault(ac => ac.Name == cardName);
			});

			//lock (choiceJob)
			//{
			//	choiceJob.Object = new Choice
			//	(
			//		ChoiceType.Play,
			//		minNumberOfSelections: 0,
			//		maxNumberOfSelections: 1,
			//		cards: cards.Select(cardMapper.ToCardDto),
			//		operations: new List<OperationType> { OperationType.Default, OperationType.Play }
			//	);

			//	choiceJob.Done = true;
			//	Monitor.Pulse(choiceJob);
			//}

			//lock (answerJob)
			//{
			//	answerJob.Done = false;

			//	while (!answerJob.Done)
			//	{
			//		Monitor.Wait(answerJob);
			//	}

			//	string cardName = answerJob.Object.Values.SingleOrDefault(c => c.OperationType == OperationType.Play)?.Card?.Name;
			//	return cards.SingleOrDefault(ac => ac.Name == cardName);
			//}
		}

		public override Card RemodelTrash(PlayerState ps, Kingdom k)
		{
			throw new NotImplementedException();
		}

		public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		{
			return LockTemplate(new Choice
			(
				type: ChoiceType.Buy,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1, // todo ps.Buys
				cards: wrapper.AvailableCards.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Buy }
			), answer =>
			{
				string cardName = answer.Values.SingleOrDefault(c => c.OperationType == OperationType.Buy)?.Card?.Name;
				return wrapper.AvailableCards.SingleOrDefault(ac => ac.Name == cardName);
			});
		}

		// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
		public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
		{
			lock (choiceJob)
			{
				choiceJob.Object = new Choice
				(
					type: ChoiceType.SpyDiscard,
					minNumberOfSelections: 1,
					maxNumberOfSelections: 1,
					cards: new List<CardDto> { cardMapper.ToCardDto(c) },
					operations: new List<OperationType> { OperationType.Discard, OperationType.PutOnTop }
				);

				choiceJob.Done = true;
				Monitor.Pulse(choiceJob);
			}

			lock (answerJob)
			{
				answerJob.Done = false;

				while (!answerJob.Done)
				{
					Monitor.Wait(answerJob);
				}

				// TODO kontrola
				OperationType operationType = answerJob.Object.Values.Single().OperationType;
				return operationType == OperationType.Discard;
			}
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
