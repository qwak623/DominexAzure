using AI.Model;
using AI.Provincial;
using Dominex.Contracts;
using Dominex.Contracts.Game;
using Dominex.Contracts.Results;
using Dominex.Contracts.ServerApi;
using Dominex.Services.Game;
using GameCore;
using GameCore.Cards;
using GameCore.Observers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Dominex.Facades.Game;

[Service]
[Authorize]
public class GameFacade : IGameFacade
{
	// todo zbavit se static věcí (singleton?)
	private static IGame Game;

	private static readonly Job<ChoiceDto> choiceJob = new();
	private static readonly Job<Answer> answerJob = new();

	private static Kingdom kingdom;
	private readonly IGameLogger gameLogger;
	private readonly IKingdomObserver kingdomObserver;
	private readonly IPlayerStateObserver playerStateObserver;
	private readonly ICardMapper cardMapper;
	private readonly IOperationMapper operationMapper;

	private static GameResultsDto gameResults; // todo tohle je tady jen temporary - pak presunout jinam asi, vyresit results id apod

	public GameFacade(
		IGameLogger gameLogger,
		IKingdomObserver kingdomObserver,
		IPlayerStateObserver playerStateObserver,
		ICardMapper cardMapper,
		IOperationMapper operationMapper)
	{
		this.gameLogger = gameLogger;
		this.kingdomObserver = kingdomObserver;
		this.playerStateObserver = playerStateObserver;
		this.cardMapper = cardMapper;
		this.operationMapper = operationMapper;
	}

	public Task StartWithCards(IEnumerable<string> cardTypes, CancellationToken cancellationToken = default)
	{
		List<Card> cards = cardTypes
			.Select(c =>
			{
				if (!Enum.TryParse<CardName>(c, out var cardType))
				{
					throw new ArgumentException($"{c} is not a valid type of card.");
				}
				return Card.Get(cardType);
			}).ToList();

		var randomAI = new Decoy();

		return Start(cards, randomAI);
	}

	public async Task<ChoiceDto> JoinGame(/*Dto<Guid> gameId, */Dto<int> playerId, CancellationToken cancellationToken = default)
	{
		await Task.Yield();

		lock (choiceJob)
		{
			while (!choiceJob.Done)
			{
				Monitor.Wait(choiceJob);
			}

			return choiceJob.Object;
		}
	}

	public async Task<ChoiceDto> Submit(Answer answer, CancellationToken cancellationToken = default)
	{
		await Task.Yield();

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

			return choiceJob.Object;
		}
	}

	// todo vymyslet jak udělat lépe request notification
	// todo notification by měla být push, ne pull
	// todo je třeba sehnat playerstate, aby se mohly ukázat slevy na kartách
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

	public Task<GameResultsDto> GetGameResults(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(gameResults);
	}

	private Answer CallClient(ChoiceDto choice)
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

		// filter out default operations
		answerJob.Object.Values = answerJob.Object.Values.Where(v => v.OperationType != OperationType.Default).ToList();

		ValidateAnswer(choice, answerJob.Object);

		return answerJob.Object;
	}

	private void CallClientWithResults(GameResults results)
	{
		// todo move to mapper
		gameResults = new GameResultsDto
		{
			PlayerResults = results.Players.Select(p => new PlayerResultsDto
			{
				Name = p.Name,
				Points = p.GetVictoryPoints(),
				Cards = p.PlayerState.DiscardPile.GroupBy(c => c.Card).Select(group => new CardResultsDto
				{
					Card = cardMapper.ToCardDto(group.Key, p.PlayerState),
					Count = group.Count(),
					PointsPerCard = group.Key.CountPoints(p),
				}).ToList(),
			}).ToList(),
		};

		lock (choiceJob)
		{
			choiceJob.Object = new ChoiceDto
			{
				Type = ChoiceType.GameEnd,
			};
			choiceJob.Done = true;
			Monitor.Pulse(choiceJob);
		}
	}

	private async Task Start(List<Card> cards, User ai)
	{
		await Task.Yield();

		if (Game is null)
		{
			kingdom = cards.GetKingdom(2, kingdomObserver);

			var humanUser = new Human(playerStateObserver, cardMapper, operationMapper, CallClient);

			var users = new User[] { humanUser, ai /*random*/ };

			Game = new GameCore.Game(users, kingdom, gameLogger);
			Game.Play().ContinueWith(async task => CallClientWithResults(await task));
		}
	}

	private void ValidateAnswer(ChoiceDto choice, Answer answer)
	{
		if (answer.Values.Count < choice.Min)
		{
			throw new ArgumentOutOfRangeException($"Minimal count of cards with non-default operation is {choice.Min} but the actual number is {answer.Values.Count}.");
		}
		if (answer.Values.Count > choice.Max)
		{
			throw new ArgumentOutOfRangeException($"Maximal count of cards with non-default operation is {choice.Max} but the actual number is {answer.Values.Count}.");
		}
		if (answer.Values.Any(v => v.Index < 0 || v.Index >= choice.Values.Count))
		{
			throw new ArgumentOutOfRangeException($"Invalid index.");
		}
		foreach (var value in answer.Values)
		{
			var correspondingChoice = choice.Values[value.Index];
			if (!correspondingChoice.Operations.Contains(value.OperationType))
			{
				throw new ArgumentException($"Chosen operation type {value.OperationType} for card {correspondingChoice.Card.Name} was not present in the options for this card.");
			}
		}
	}
}