using Dominex.Contracts.Game;
using GameCore.Cards;
using GameCore.Observers;
using GameCore;
using GameCore.Cards.Base;
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
	public override string GetName() => "Todo Name";

	// todo ps - možná by mělo být jen card selection
	// todo kingdom - možná by to nemuselo být tady
	public override Card BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand.Where(c => c.IsVictory).ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.BureaucratPutOnTop,
			min: 1,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.PutOnTop },
			message: "Choose a Victory card to put onto your draw pile."
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override List<Card> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.CellarDiscard,
			min: 0,
			max: cardSelection.Count,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard },
			message: "Discard any number of cards."
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}

	public override bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.ChancellorDiscard,
			min: 0,
			max: 1,
			cards: new List<CardDto>() { null }, // TODO NullReferenceException
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
		));

		return answer.Values.Any();
	}

	public override List<Card> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.ChapelTrash,
			min: 0,
			max: Math.Min(ps.Hand.Count, 4), // todo můžeme zahodit kapli?
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}


	public override bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, Card c)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.LibrarySkip,
			min: 0,
			max: 1,
			cards: new List<CardDto>() { cardMapper.ToCardDtoWithIndex(c, 0) },
			operations: new List<OperationType> { OperationType.Default, OperationType.Skip }
		));

		return answer.Values.Any();
	}

	public override List<Card> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.MilitiaDiscard,
			min: discardCount,
			max: discardCount,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}

	public override Card MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<Card> cardSelection)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.MineTrash,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState playerState, Kingdom kingdom)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			ChoiceType.MoatDefend,
			min: 0,
			max: 1,
			cards: new List<CardDto>() { cardMapper.ToCardDto(Copper.Get()) },
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Any();
	}

	public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null)
	{
		var cardSelection = ps.Hand.Where(p => p.IsAction).ToList();

		// todo je třeba vyřešit atacking card, asi ideálně přidat do choice
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: null,
			ChoiceType.Play,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Play }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override Card RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			type: ChoiceType.RemodelTrash, // todo nemůžeme remodelovat sám sebe
			min: 0, // todo - neodpovida description - opravit
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var cardSelection = wrapper.AvailableCards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: null,
			type: phase == Phase.Buy ? ChoiceType.Buy : ChoiceType.Gain,
			min: 0,
			max: 1, // todo ps.Buys - více buyu najednou
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, phase == Phase.Buy ? OperationType.Buy : OperationType.Gain }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, Card c, Phase p)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			type: ChoiceType.SpyDiscard,
			min: 1,
			max: 1,
			cards: new List<CardDto> { cardMapper.ToCardDtoWithIndex(c, 0) },
			operations: new List<OperationType> { OperationType.Discard, OperationType.PutOnTop }
		));

		OperationType operationType = answer.Values.Single().OperationType;
		return operationType == OperationType.Discard;
	}

	public override Card ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			type: ChoiceType.ThiefChoose,
			min: 1,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Choose },
			message: "Choose an opponent's treasure to steal or trash." // todo funguje pro dva hrače, pro vice by chtělo jmeno
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, Card c)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			type: ChoiceType.ThiefSteal,
			min: 1,
			max: 1,
			cards: new List<CardDto> { cardMapper.ToCardDtoWithIndex(c, 0) },
			operations: new List<OperationType> { OperationType.Trash, OperationType.Steal },
			message: "Trash or steal this card."
		));

		return answer.Values.Single().OperationType == OperationType.Steal;
	}

	public override Card ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			cardPlayed: cardMapper.ToCardDto(cardPlayed),
			type: ChoiceType.ThroneRoomPlay,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDtoWithIndex),
			operations: new List<OperationType> { OperationType.Default, OperationType.Play }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}
}