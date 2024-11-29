using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.ServerApi;
using Dominex.Services.Game;
using GameCore;
using GameCore.Cards;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Org.BouncyCastle.Crypto;

namespace Dominex.Facades.Game;

[Service]
//[Authorize(Roles = nameof(Role.Entry.SystemAdministrator))]
public class GameFacade : IGameFacade
{
	// todo zbavit se static věcí (singleton?)
	private static GameCore.Game Game;

	private static Job<Choice> choiceJob = new();

	private static Job<Answer> answerJob = new();
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
		await Task.Run(Game.RequestPlayerNotifications, cancellationToken);
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
		public override string GetName() => "Todo Name";

		private Answer CallClient(Choice choice)
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
			}

			answerJob.Object.Values = answerJob.Object.Values.Where(v => v.OperationType != OperationType.Default).ToList();

			// todo kontroly - extrahovat do metody

			if (answerJob.Object.Values.Count < choice.MinNumberOfSelections)
			{
				throw new ArgumentOutOfRangeException($"Minimal count of cards with non-default operation is {choice.MinNumberOfSelections} but actual number is {answerJob.Object.Values.Count}.");
			}
			if (answerJob.Object.Values.Count > choice.MaxNumberOfSelections)
			{
				throw new ArgumentOutOfRangeException($"Maximal count of cards with non-default operation is {choice.MaxNumberOfSelections} but actual number is {answerJob.Object.Values.Count}.");
			}
			if (answerJob.Object.Values.Any(v => v.Index < 0 || v.Index >= choice.Values.Count))
			{
				throw new ArgumentOutOfRangeException($"Invalid index.");
			}
			foreach (var value in answerJob.Object.Values)
			{
				var correspondingChoice = choice.Values[value.Index];
				if (!correspondingChoice.Operations.Contains(value.OperationType))
				{
					throw new ArgumentException($"Chosen operation type {value.OperationType} for card {correspondingChoice.Card.Name} was not present in the options for this card.");
				}
			}

			return answerJob.Object;
		}

		public override Card BureaucratPutOnTop(PlayerState ps, Kingdom k)
		{
			var cardSelection = ps.Hand.Where(c => c.IsVictory).ToList();

			var answer = CallClient(new Choice
			(
				ChoiceType.BureaucratPutOnTop,
				minNumberOfSelections: 1,
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.PutOnTop }
			));

			return cardSelection[answer.Values.Single().Index];
		}

		public override List<Card> CellarDiscard(PlayerState ps, Kingdom k)
		{
			var cardSelection = ps.Hand;

			var answer = CallClient(new Choice
			(
				ChoiceType.CellarDiscard,
				minNumberOfSelections: 0,
				maxNumberOfSelections: ps.Hand.Count - 1, // todo lze discardnout cellar?
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
			));

			return answer.Values.Select(c => cardSelection[c.Index]).ToList();
		}

		public override bool ChancellorDiscard(PlayerState ps, Kingdom k)
		{
			var answer = CallClient(new Choice
			(
				ChoiceType.ChancellorDiscard,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: new List<CardDto>() { null },
				operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
			));

			return answer.Values.Any();
		}

		public override List<Card> ChapelTrash(PlayerState ps, Kingdom k)
		{
			var cardSelection = ps.Hand;

			var answer = CallClient(new Choice
			(
				ChoiceType.ChapelTrash,
				minNumberOfSelections: 0,
				maxNumberOfSelections: Math.Min(ps.Hand.Count, 4), // todo můžeme zahodit kapli?
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
			));

			return answer.Values.Select(c => cardSelection[c.Index]).ToList();
		}


		public override bool LibrarySkip(PlayerState ps, Kingdom k, Card c)
		{
			var answer = CallClient(new Choice
			(
				ChoiceType.LibrarySkip,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: new List<CardDto>() { cardMapper.ToCardDto(c, -1) },
				operations: new List<OperationType> { OperationType.Default, OperationType.Skip }
			));

			return answer.Values.Any();
		}

		public override List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount)
		{
			var cardSelection = ps.Hand;

			var answer = CallClient(new Choice
			(
				ChoiceType.MilitiaDiscard,
				minNumberOfSelections: discardCount,
				maxNumberOfSelections: discardCount,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
			));

			return answer.Values.Select(c => cardSelection[c.Index]).ToList();
		}

		public override Card MineTrash(PlayerState ps, Kingdom k)
		{
			var cardSelection = ps.Hand.Where(c => c.IsTreasure).ToList();
			var answer = CallClient(new Choice
			(
				ChoiceType.MineTrash,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
			));

			return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
		}

		public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null)
		{
			var cardSelection = ps.Hand.Where(p => p.IsAction).ToList();

			// todo je třeba vyřešit atacking card, asi ideálně přidat do choice
			var answer = CallClient(new Choice
			(
				ChoiceType.Play,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Play }
			));

			return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
		}

		public override Card RemodelTrash(PlayerState ps, Kingdom k)
		{
			var cardSelection = ps.Hand;

			var answer = CallClient(new Choice
			(
				type: ChoiceType.RemodelTrash, // todo nemůžeme remodelovat sám sebe
				minNumberOfSelections: 0, // todo - neodpovida description - opravit
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
			));

			return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
		}

		public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		{
			var cardSelection = wrapper.AvailableCards.ToList();

			var answer = CallClient(new Choice
			(
				type: ChoiceType.Buy,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1, // todo ps.Buys - více buyu najednou
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Buy }
			));

			return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
		}

		// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
		public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
		{
			var answer = CallClient(new Choice
			(
				type: ChoiceType.SpyDiscard,
				minNumberOfSelections: 1,
				maxNumberOfSelections: 1,
				cards: new List<CardDto> { cardMapper.ToCardDto(c, -1) },
				operations: new List<OperationType> { OperationType.Discard, OperationType.PutOnTop }
			));

			OperationType operationType = answer.Values.Single().OperationType;
			return operationType == OperationType.Discard;
		}

		public override Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			var cardSelection = cards.ToList();

			var answer = CallClient(new Choice
			(
				type: ChoiceType.ThiefChoose,
				minNumberOfSelections: 1,
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Choose }
			));

			return cardSelection[answer.Values.Single().Index];
		}

		public override bool ThiefSteal(PlayerState ps, Kingdom k, Card c)
		{
			var answer = CallClient(new Choice
			(
				type: ChoiceType.ThiefSteal,
				minNumberOfSelections: 1,
				maxNumberOfSelections: 1,
				cards: new List<CardDto> { cardMapper.ToCardDto(c, 0) },
				operations: new List<OperationType> { OperationType.Trash, OperationType.Steal }
			));

			return answer.Values.Single().OperationType == OperationType.Steal;
		}

		public override Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			var cardSelection = cards.ToList();

			var answer = CallClient(new Choice
			(
				type: ChoiceType.ThroneRoomPlay,
				minNumberOfSelections: 0,
				maxNumberOfSelections: 1,
				cards: cardSelection.Select(cardMapper.ToCardDto),
				operations: new List<OperationType> { OperationType.Default, OperationType.Play }
			));

			return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
		}
	}

	public class RandomUser : GameCore.User
	{
		public override Card BureaucratPutOnTop(PlayerState ps, Kingdom k)
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
			return null;
		}

		public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null)
		{
			return null;
		}

		public override Card RemodelTrash(PlayerState ps, Kingdom k)
		{
			return null;
		}

		public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		{
			return wrapper.AvailableCards.First();
		}

		public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
		{
			return false;
		}

		public override Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			return null;
		}

		public override bool ThiefSteal(PlayerState ps, Kingdom k, Card c)
		{
			return true;
		}

		public override Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
		{
			return null;
		}
	}
}