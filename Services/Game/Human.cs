using Dominex.Contracts.Game;
using GameCore;
using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.Observers;

namespace Dominex.Services.Game;
public class Human(IPlayerStateObserver playerStateObserver, ICardMapper cardMapper, IOperationMapper operationMapper, Func<ChoiceDto, Answer> callClient) : User
{
	private readonly IPlayerStateObserver playerStateObserver = playerStateObserver;
	private readonly ICardMapper cardMapper = cardMapper;
	private readonly IOperationMapper operationMapper = operationMapper;
	private readonly Func<ChoiceDto, Answer> CallClient = callClient;

	public override IPlayerStateObserver GetPlayerStateObserver()
	{
		return playerStateObserver;
	}

	public override CardInstance PlayCard(List<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null)
		// todo je třeba vyřešit attacking card, asi ideálně přidat do choice
		=> AskForCards(null, ps, cards, ChoiceType.Play, OperationType.Play, 0, 1).SingleOrDefault();

	public override string GetName() => "Todo Name";

	// todo ps - možná by mělo být jen card selection
	// todo kingdom - možná by to nemuselo být tady

	#region cards base
	public override CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.BureaucratPutOnTop, OperationType.PutOnTop, 1, 1,
			message: "Choose a Victory card to put onto your draw pile.").Single();

	public override List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.CellarDiscard, OperationType.Discard, 0, cardSelection.Count,
			message: "Discard any number of cards.");

	public override bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> AskYesNo(cardPlayed, ps, ChoiceType.ChancellorDiscard, OperationType.Discard, [null]); // TODO NullReferenceException

	public override List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.ChapelTrash, OperationType.Trash, 0, Math.Min(cardSelection.Count, 4));

	public override bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> AskYesNo(cardPlayed, ps, ChoiceType.LibrarySkip, OperationType.Skip, [cardMapper.ToCardDtoWithIndex(c, 0, ps)]);

	public override List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.MilitiaDiscard, OperationType.Discard, discardCount, discardCount);

	public override CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.MineTrash, OperationType.Trash, 0, 1).SingleOrDefault();

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState playerState, Kingdom kingdom)
		=> AskYesNo(cardPlayed, playerState, ChoiceType.MoneylenderTrash, OperationType.Trash,
			[cardMapper.ToCardDto(Copper.Get(), playerState)]);

	public override CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		// todo nemůžeme remodelovat sám sebe
		// todo min: 0 - neodpovida description - opravit
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.RemodelTrash, OperationType.Trash, 0, 1).SingleOrDefault();

	public override CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		var cardSelection = wrapper.AvailableCards.ToList();

		// buying is always optional (a buy can go unspent); every other caller of this method
		// is a mandatory "gain a card" effect (Workshop, Mine, Remodel, Feast, ...), so it can
		// only be declined when there's genuinely nothing available to gain
		var min = phase == Phase.Buy || cardSelection.Count == 0 ? 0 : 1;
		var type = phase == Phase.Buy ? ChoiceType.Buy : ChoiceType.Gain;
		var op = phase == Phase.Buy ? OperationType.Buy : OperationType.Gain;

		// todo ps.Buys - více buyu najednou
		return AskForCards(null, ps, cardSelection, type, op, min, 1).SingleOrDefault();
	}

	// unlike SelectCardToGain, min is always 0 here - "may gain" cards (e.g. Saboteur) can be
	// declined even when a valid target exists
	public override CardInstance SelectOptionalCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
		=> AskForCards(null, ps, wrapper.AvailableCards.ToList(), ChoiceType.Gain, OperationType.Gain, 0, 1).SingleOrDefault();

	// todo lepší description - potřebujeme vědět, čí kartu zahazujeme
	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p)
		=> AskOperations(cardPlayed, ps, ChoiceType.SpyDiscard, [cardMapper.ToCardDtoWithIndex(c, 0, ps)],
			[OperationType.Discard, OperationType.PutOnTop], 1, 1).Single() == OperationType.Discard;

	public override CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		// todo funguje pro dva hrače, pro vice by chtělo jmeno
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.ThiefChoose, OperationType.Choose, 1, 1,
			message: "Choose an opponent's treasure to steal or trash.").Single();

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> AskOperations(cardPlayed, ps, ChoiceType.ThiefSteal, [cardMapper.ToCardDtoWithIndex(c, 0, ps)],
			[OperationType.Trash, OperationType.Steal], 1, 1, message: "Trash or steal this card.").Single() == OperationType.Steal;

	public override CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.ThroneRoomPlay, OperationType.Play, 0, 1).SingleOrDefault();
	#endregion cards base

	#region cards intrique
	public override bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom kingdom)
		=> AskYesNo(cardPlayed, ps, ChoiceType.BaronDiscard, OperationType.Discard, [cardMapper.ToCardDto(Estate.Get(), ps)]);

	public override CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.CourtyardPutOnTop, OperationType.PutOnTop, 1, 1).SingleOrDefault();

	public override CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.MasqueradePass, OperationType.Pass, 1, 1).Single();

	public override CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.MasqueradeTrash, OperationType.Trash, 0, 1).SingleOrDefault();

	public override bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> AskYesNo(cardPlayed, ps, ChoiceType.MasqueradeTrash, OperationType.Trash, [cardMapper.ToCardDtoWithIndex(c, 0, ps)]);

	public override bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		// todo potential null reference exception
		// todo better options - discard or gain 2 coins
		=> AskYesNo(cardPlayed, ps, ChoiceType.MinionDiscard, OperationType.Discard, [null]);

	public override bool NoblesChooseCards(Card cardPlayed, PlayerState ps, Kingdom kingdom)
		// todo potential null reference exception
		// todo better options - draw 3 cards or gain 2 actions
		=> AskYesNo(cardPlayed, ps, ChoiceType.MinionDiscard, OperationType.Pass, [null]);

	public override bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k)
		// todo potential null reference exception
		=> AskOperations(cardPlayed, ps, ChoiceType.TorturerChoose, [null], [OperationType.Gain, OperationType.Discard],
			0, 1).Single() == OperationType.Discard;

	public override List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.TorturerDiscard, OperationType.Discard, discardCount, discardCount);

	public override List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.TradingPostTrash, OperationType.Trash, 2, 2);

	public override List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.SecretChamberDiscard, OperationType.Discard, 0, cardSelection.Count);

	// TODO any order!
	public override List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.SecretChamberPutOnDeck, OperationType.PutOnTop, count, count);

	public override List<CardInstance> ScoutOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
	{
		// TODO any order!
		throw new NotImplementedException();
	}

	public override List<CardInstance> DiplomatDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.DiplomatDiscard, OperationType.Discard, count, count);

	public override bool LurkerTrash(Card cardPlayed, PlayerState ps, Kingdom k)
		// todo potential null reference exception
		=> AskOperations(cardPlayed, ps, ChoiceType.LurkerChoose, [null], [OperationType.Gain, OperationType.Trash], 1, 1, message: "Trash or gain this card.").Single() == OperationType.Trash;

	public override CardInstance LurkerChooseCardToTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.LurkerTrash, OperationType.Trash, 1, 1).Single();

	public override CardInstance LurkerChooseCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.LurkerGain, OperationType.Gain, 1, 1).Single();

	public override bool MillWantsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> AskYesNo(cardPlayed, ps, ChoiceType.MillDiscard, OperationType.Discard);

	public override List<CardInstance> MillChooseCardsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards, int count)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.MillDiscard, OperationType.Discard, count, count);


	public override List<CardInstance> PatrolOrderCards(Card cardPlayed, PlayerState playerState, Kingdom kingdom, List<CardInstance> cards)
	{
		throw new NotImplementedException();
	}

	public override CardInstance ReplaceTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> AskForCards(cardPlayed, ps, cardSelection, ChoiceType.ReplaceTrash, OperationType.Trash, 1, 1).Single();
	public override CardInstance CourtierReveal(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> AskForCards(cardPlayed, ps, cards, ChoiceType.CourtierReveal, OperationType.Reveal, 1, 1).Single();
	public override List<CourtierBenefit> CourtierChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<CourtierBenefit> availableBenefits)
		=> operationMapper.ToCourtierBenefits(AskOperations(cardPlayed, ps, ChoiceType.CourtierChooseBenefits, [null],
			operationMapper.ToOperationTypes(availableBenefits), benefitCount, benefitCount));
	public override List<PawnBenefit> PawnChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<PawnBenefit> availableBenefits)
		=> operationMapper.ToPawnBenefits(AskOperations(cardPlayed, ps, ChoiceType.PawnChooseBenefits, [null],
			operationMapper.ToOperationTypes(availableBenefits), benefitCount, benefitCount));
	#endregion cards intrique

	/// <summary>
	/// Asks the client to pick between min and max cards out of selection. Cards not picked are
	/// implicitly OperationType.Default; picked ones are op. Returns the picked CardInstances,
	/// resolved back from selection by index.
	/// </summary>
	private List<CardInstance> AskForCards(Card cardPlayed, PlayerState ps, IReadOnlyList<CardInstance> selection,
		ChoiceType choiceType, OperationType op, int min, int max, string message = null)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed is null ? null : cardMapper.ToCardDto(cardPlayed, ps),
			choiceType,
			min,
			max,
			selection.Select((c, i) => cardMapper.ToCardDtoWithIndex(c, i, ps)),
			[OperationType.Default, op],
			message
		));

		return [.. answer.Values.Select(c => selection[c.Index])];
	}

	/// <summary>
	/// Asks a plain yes/no question (op vs. not doing it). cards is what's shown alongside the
	/// question, if anything - most of these don't have a real card selection, just a single
	/// accept/decline choice.
	/// </summary>
	private bool AskYesNo(Card cardPlayed, PlayerState ps, ChoiceType choiceType, OperationType op,
		IEnumerable<CardDto> cards = null, string message = null)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed is null ? null : cardMapper.ToCardDto(cardPlayed, ps),
			choiceType,
			min: 0,
			max: 1,
			cards: cards ?? [],
			operations: [OperationType.Default, op],
			message: message
		));

		return answer.Values.Count != 0;
	}

	/// <summary>
	/// Asks the client to pick one of several named operations for a single card (e.g. trash vs.
	/// steal). Returns whether trueOp was the one picked.
	/// </summary>
	private List<OperationType> AskOperations(Card cardPlayed, PlayerState ps, ChoiceType choiceType, IEnumerable<CardDto> cards,
		List<OperationType> operations, int min, int max, string message = null)
	{
		var answer = CallClient(new ChoiceDto
		(
			cardPlayed is null ? null : cardMapper.ToCardDto(cardPlayed, ps),
			choiceType,
			min,
			max,
			cards,
			operations,
			message
		));

		return [.. answer.Values.Select(c => operations[c.Index])];
	}
}
