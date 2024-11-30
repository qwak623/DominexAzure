using Dominex.Contracts.Game;
using GameCore.Cards;
using GameCore.Observers;
using GameCore;

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
	public override Card BureaucratPutOnTop(PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand.Where(c => c.IsVictory).ToList();

		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.BureaucratPutOnTop,
			min: 1,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.PutOnTop }
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override List<Card> CellarDiscard(PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.CellarDiscard,
			min: 0,
			max: cardSelection.Count, // todo lze discardnout cellar?
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}

	public override bool ChancellorDiscard(PlayerState ps, Kingdom k)
	{
		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.ChancellorDiscard,
			min: 0,
			max: 1,
			cards: new List<CardDto>() { null },
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
		));

		return answer.Values.Any();
	}

	public override List<Card> ChapelTrash(PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.ChapelTrash,
			min: 0,
			max: Math.Min(ps.Hand.Count, 4), // todo můžeme zahodit kapli?
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}


	public override bool LibrarySkip(PlayerState ps, Kingdom k, Card c)
	{
		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.LibrarySkip,
			min: 0,
			max: 1,
			cards: new List<CardDto>() { cardMapper.ToCardDto(c, 0) },
			operations: new List<OperationType> { OperationType.Default, OperationType.Skip }
		));

		return answer.Values.Any();
	}

	public override List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.MilitiaDiscard,
			min: discardCount,
			max: discardCount,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Discard }
		));

		return answer.Values.Select(c => cardSelection[c.Index]).ToList();
	}

	public override Card MineTrash(PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand.Where(c => c.IsTreasure).ToList();
		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.MineTrash,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null)
	{
		var cardSelection = ps.Hand.Where(p => p.IsAction).ToList();

		// todo je třeba vyřešit atacking card, asi ideálně přidat do choice
		var answer = CallClient(new ChoiceDto
		(
			ChoiceType.Play,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Play }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override Card RemodelTrash(PlayerState ps, Kingdom k)
	{
		var cardSelection = ps.Hand;

		var answer = CallClient(new ChoiceDto
		(
			type: ChoiceType.RemodelTrash, // todo nemůžeme remodelovat sám sebe
			min: 0, // todo - neodpovida description - opravit
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Trash }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var cardSelection = wrapper.AvailableCards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			type: phase == Phase.Buy ? ChoiceType.Buy : ChoiceType.Gain,
			min: 0,
			max: 1, // todo ps.Buys - více buyu najednou
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, phase == Phase.Buy ? OperationType.Buy : OperationType.Gain }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}

	// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
	public override bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p)
	{
		var answer = CallClient(new ChoiceDto
		(
			type: ChoiceType.SpyDiscard,
			min: 1,
			max: 1,
			cards: new List<CardDto> { cardMapper.ToCardDto(c, 0) },
			operations: new List<OperationType> { OperationType.Discard, OperationType.PutOnTop }
		));

		OperationType operationType = answer.Values.Single().OperationType;
		return operationType == OperationType.Discard;
	}

	public override Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			type: ChoiceType.ThiefChoose,
			min: 1,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Choose }
		));

		return cardSelection[answer.Values.Single().Index];
	}

	public override bool ThiefSteal(PlayerState ps, Kingdom k, Card c)
	{
		var answer = CallClient(new ChoiceDto
		(
			type: ChoiceType.ThiefSteal,
			min: 1,
			max: 1,
			cards: new List<CardDto> { cardMapper.ToCardDto(c, 0) },
			operations: new List<OperationType> { OperationType.Trash, OperationType.Steal }
		));

		return answer.Values.Single().OperationType == OperationType.Steal;
	}

	public override Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		var cardSelection = cards.ToList();

		var answer = CallClient(new ChoiceDto
		(
			type: ChoiceType.ThroneRoomPlay,
			min: 0,
			max: 1,
			cards: cardSelection.Select(cardMapper.ToCardDto),
			operations: new List<OperationType> { OperationType.Default, OperationType.Play }
		));

		return answer.Values.Any() ? cardSelection[answer.Values.Single().Index] : null;
	}
}