using Dominex.Contracts.Game;
using GameCore.Cards;
using GameCore.Observers;
using GameCore;
using GameCore.Cards.GeneralCards;

namespace Dominex.Services.Game;
public class Human : User, IHuman
{
	private readonly IPlayerStateObserver playerStateObserver;
	private readonly ICardMapper cardMapper;
	private readonly Func<ChoiceDto, Answer> CallClient;

	public Human(IPlayerStateObserver playerStateObserver, ICardMapper cardMapper, Func<ChoiceDto, Answer> callClient)
	{
		this.playerStateObserver = playerStateObserver;
		this.cardMapper = cardMapper;
		CallClient = callClient;
	}

	public override IPlayerStateObserver GetPlayerStateObserver()
	{
		return playerStateObserver;
	}

	public override CardInstance PlayCard(IEnumerable<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null)
	{
		var cardSelection = ps.Hand.Where(p => p.Card.IsAction).ToList();

		// todo je třeba vyřešit attacking card, asi ideálně přidat do choice
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: null,
			ChoiceType.Play,
			min: 0,
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Play]
		));

		return answer.Values.Count != 0 ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override string GetName() => "Todo Name";

	// todo ps - možná by mělo být jen card selection
	// todo kingdom - možná by to nemuselo být tady

	#region cards base
	public override CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand.Where(c => c.IsVictory).ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.BureaucratPutOnTop,
			min: 1,
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.PutOnTop],
			message: "Choose a Victory card to put onto your draw pile."
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.CellarDiscard,
			min: 0,
			max: cardSelection.Count,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Discard],
			message: "Discard any number of cards."
		));

		return [.. answer.Values.Select(c => cardSelection[c.Index])];
	}

	public override bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.ChancellorDiscard,
			min: 0,
			max: 1,
			cards: [null], // TODO NullReferenceException
			operations: [OperationType.Default, OperationType.Discard]
		));

		return answer.Values.Count != 0;
	}

	public override List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.ChapelTrash,
			min: 0,
			max: Math.Min(ps.Hand.Count, 4), // todo můžeme zahodit kapli?
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Trash]
		));

		return [.. answer.Values.Select(c => cardSelection[c.Index])];
	}


	public override bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.LibrarySkip,
			min: 0,
			max: 1,
			cards: [cardMapper.ToCardDtoWithIndex(c, 0, ps)],
			operations: [OperationType.Default, OperationType.Skip]
		));

		return answer.Values.Count != 0;
	}

	public override List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.MilitiaDiscard,
			min: discardCount,
			max: discardCount,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Discard]
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}

	public override CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<CardInstance> cardSelection)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.MineTrash,
			min: 0,
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Trash]
		));

		return answer.Values.Count != 0 ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState playerState, Kingdom kingdom)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, playerState),
			ChoiceType.MoneylenderTrash,
			min: 0,
			max: 1,
			cards: [cardMapper.ToCardDto(Copper.Get(), playerState)],
			operations: [OperationType.Default, OperationType.Trash]
		));

		return answer.Values.Count != 0;
	}

	public override CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.RemodelTrash, // todo nemůžeme remodelovat sám sebe
			min: 0, // todo - neodpovida description - opravit
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Trash]
		));

		return answer.Values.Count != 0 ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var cardSelection = wrapper.AvailableCards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: null,
			type: phase == Phase.Buy ? ChoiceType.Buy : ChoiceType.Gain,
			min: 0,
			max: 1, // todo ps.Buys - více buyu najednou
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, phase == Phase.Buy ? OperationType.Buy : OperationType.Gain]
		));

		return answer.Values.Count != 0 ? cardSelection[answer.Values.Single().Index] : null;
	}

	// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.SpyDiscard,
			min: 1,
			max: 1,
			cards: new List<CardDto> { cardMapper.ToCardDtoWithIndex(c, 0, ps) },
			operations: [OperationType.Discard, OperationType.PutOnTop]
		));

		OperationType operationType = answer.Values.Single().OperationType;
		return operationType == OperationType.Discard;
	}

	public override CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.ThiefChoose,
			min: 1,
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Choose],
			message: "Choose an opponent's treasure to steal or trash." // todo funguje pro dva hrače, pro vice by chtělo jmeno
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.ThiefSteal,
			min: 1,
			max: 1,
			cards: [cardMapper.ToCardDtoWithIndex(c, 0, ps)],
			operations: [OperationType.Trash, OperationType.Steal],
			message: "Trash or steal this card."
		));

		return answer.Values.Single().OperationType == OperationType.Steal;
	}

	public override CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.ThroneRoomPlay,
			min: 0,
			max: 1,
			cards: cardSelection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Play]
		));

		return answer.Values.Count != 0 ? cardSelection[answer.Values.Single().Index] : null;
	}
	#endregion cards base

	#region cards intrique
	public override bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom kingdom)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.BaronDiscard,
			min: 0,
			max: 1,
			cards: [cardMapper.ToCardDto(Estate.Get(), ps)],
			operations: [OperationType.Default, OperationType.Discard]
		));

		return answer.Values.Count != 0;
	}

	public override CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			ChoiceType.CourtyardPutOnTop,
			min: 1,
			max: 1,
			cards: cards.Select(c => cardMapper.ToCardDtoWithIndex(c, 0, ps)),
			operations: [OperationType.Default, OperationType.PutOnTop]
		));

		return answer.Values.Count != 0 ? cards.ElementAt(answer.Values.Single().Index) : null;
	}

	public override CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.MasqueradePass,
			min: 1,
			max: 1,
			cards: cards.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Pass]
		));

		return cards.ElementAt(answer.Values.Single().Index);
	}

	public override CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed, ps),
			type: ChoiceType.MasqueradeTrash,
			min: 0,
			max: 1,
			cards: cards.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			operations: [OperationType.Default, OperationType.Trash]
		));

		return answer.Values.Count != 0 ? cards.ElementAt(answer.Values.Single().Index) : null;
	}
	#endregion cards intrique
}