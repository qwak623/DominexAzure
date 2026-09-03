using GameCore.Cards;
using GameCore.Cards.Intrique;
using GameCore.Observers;

namespace GameCore.GameCore;
public class UserProxy(IUser user) : IUser
{
	private readonly IUser _user = user;

	public string GetName()
	{
		return _user.GetName();
	}
	public IPlayerStateObserver GetPlayerStateObserver()
	{
		return _user.GetPlayerStateObserver();
	}
	public void SetCanCelationTokenSource(CancellationTokenSource tokenSource)
	{
		_user.SetCanCelationTokenSource(tokenSource);
	}


	public CardInstance PlayActionCard(PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.PlayActionCard(ps, k, cards), cards, required: false);
	public CardInstance PlayReactionCard(Card attackingCard, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.PlayReactionCard(attackingCard, ps, k, cards), cards, required: false);

	public CardInstance SelectCardToBuy(PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.SelectCardToBuy(ps, k, cards), cards, required: true);
	public CardInstance SelectCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.SelectCardToGain(cardPlayed, ps, k, cards), cards, required: true);
	public CardInstance SelectOptionalCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.SelectOptionalCardToGain(cardPlayed, ps, k, cards), cards, required: false);

	#region cards base
	public CardInstance ArtisanPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.ArtisanPutOnTop(cardPlayed, ps, k, cards), cards, required: true);

	public CardInstance BanditTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.BanditTrash(cardPlayed, ps, k, cards), cards, required: true);

	public CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickOne(() => _user.BureaucratPutOnTop(cardPlayed, ps, k, cardSelection), cardSelection, required: true);

	public List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickSome(() => _user.CellarDiscard(cardPlayed, ps, k, cardSelection), cardSelection, 0, cardSelection.Count);

	public CardInstance HarbingerPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickOne(() => _user.HarbingerPutOnTop(cardPlayed, ps, k, cardSelection), cardSelection, required: false);

	public bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.ChancellorDiscard(cardPlayed, ps, k);

	public List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickSome(() => _user.ChapelTrash(cardPlayed, ps, k, cardSelection), cardSelection, 0, 4);

	public bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> _user.LibrarySkip(cardPlayed, ps, k, c);

	public List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount)
		=> PickSome(() => _user.MilitiaDiscard(cardPlayed, ps, k, cardSelection, discardCount), cardSelection, discardCount, discardCount);

	public CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickOne(() => _user.MineTrash(cardPlayed, ps, k, cardSelection), cardSelection, required: false);

	public bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.MoneylenderTrash(cardPlayed, ps, k);

	public List<CardInstance> PoacherDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount)
		=> PickSome(() => _user.PoacherDiscard(cardPlayed, ps, k, cardSelection, discardCount), cardSelection, discardCount, discardCount);

	public CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickOne(() => _user.RemodelTrash(cardPlayed, ps, k, cardSelection), cardSelection, required: true);

	public List<CardInstance> SentryDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickSome(() => _user.SentryDiscard(cardPlayed, ps, k, cards), cards, 0, 2);

	public List<CardInstance> SentryOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> Order(() => _user.SentryOrderCards(cardPlayed, ps, k, cards), cards);

	public List<CardInstance> SentryTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickSome(() => _user.SentryTrash(cardPlayed, ps, k, cards), cards, 0, 2);

	public bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p)
		=> _user.SpyDiscard(cardPlayed, ps, k, c, p);

	public CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.ThiefChoose(cardPlayed, ps, k, cards), cards, required: true);

	public bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> _user.ThiefSteal(cardPlayed, ps, k, c);

	public CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.ThroneRoomPlay(cardPlayed, ps, k, cards), cards, required: false);

	public bool VassalPlay(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance card)
		=> _user.VassalPlay(cardPlayed, ps, k, card);
	#endregion cards base

	#region cards intrique
	public bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.BaronDiscard(cardPlayed, ps, k);

	public List<CourtierBenefit> CourtierChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<CourtierBenefit> availableBenefits)
		=> PickSome(() => _user.CourtierChooseBenefits(cardPlayed, ps, k, benefitCount, availableBenefits), availableBenefits, benefitCount, benefitCount);

	public CardInstance CourtierReveal(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.CourtierReveal(cardPlayed, ps, k, cards), cards, required: true);

	public CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.CourtyardPutOnTop(cardPlayed, ps, k, cards), cards, required: true);

	public List<CardInstance> DiplomatDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count)
		=> PickSome(() => _user.DiplomatDiscard(cardPlayed, ps, k, cardSelection, count), cardSelection, 3, 3);

	public CardInstance LurkerChooseCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.LurkerChooseCardToGain(cardPlayed, ps, k, cards), cards, required: true);

	public CardInstance LurkerChooseCardToTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.LurkerChooseCardToTrash(cardPlayed, ps, k, cards), cards, required: true);

	public bool LurkerTrash(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.LurkerTrash(cardPlayed, ps, k);

	public CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.MasqueradePass(cardPlayed, ps, k, cards), cards, required: true);

	public CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.MasqueradeTrash(cardPlayed, ps, k, cards), cards, required: false);

	public List<CardInstance> MillChooseCardsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards, int count)
		=> PickSome(() => _user.MillChooseCardsToDiscard(cardPlayed, ps, k, cards, count), cards, count, count);

	public bool MillWantsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.MillWantsToDiscard(cardPlayed, ps, k);

	public bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
		=> _user.MiningVillageTrash(cardPlayed, ps, k, c);

	public bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.MinionDiscard(cardPlayed, ps, k);

	public bool NoblesChooseCards(Card cardPlayed, PlayerState playerState, Kingdom kingdom)
		=> _user.NoblesChooseCards(cardPlayed, playerState, kingdom);

	public List<CardInstance> PatrolOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> Order(() => _user.PatrolOrderCards(cardPlayed, ps, k, cards), cards);

	public List<PawnBenefit> PawnChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<PawnBenefit> availableBenefits)
		=> PickSome(() => _user.PawnChooseBenefits(cardPlayed, ps, k, benefitCount, availableBenefits), availableBenefits, benefitCount, benefitCount);

	public CardInstance ReplaceTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.ReplaceTrash(cardPlayed, ps, k, cards), cards, required: true);

	public List<CardInstance> ScoutOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> Order(() => _user.ScoutOrderCards(cardPlayed, ps, k, cards), cards);

	public List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickSome(() => _user.SecretChamberDiscard(cardPlayed, ps, k, cardSelection), cardSelection, 0, cardSelection.Count);

	public List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count)
		=> PickSome(() => _user.SecretChamberPutOnDeck(cardPlayed, ps, k, cardSelection, count), cardSelection, count, count);

	public CardInstance SecretPassageChooseCard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.SecretPassageChooseCard(cardPlayed, ps, k, cards), cards, required: true);

	public StewardBenefit StewardChooseBenefit(Card cardPlayed, PlayerState ps, Kingdom k, List<StewardBenefit> allBenefits)
		=> PickOne(() => _user.StewardChooseBenefit(cardPlayed, ps, k, allBenefits), allBenefits, required: true);

	public List<CardInstance> StewardChooseCardsToTrash(Card cardPlayed, PlayerState ps, Kingdom k, int count, List<CardInstance> cards)
		=> PickSome(() => _user.StewardChooseCardsToTrash(cardPlayed, ps, k, count, cards), cards, count, count);

	public bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k)
		=> _user.TorturerChooseCurse(cardPlayed, ps, k);

	public List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount)
		=> PickSome(() => _user.TorturerDiscard(cardPlayed, ps, k, cardSelection, discardCount), cardSelection, discardCount, discardCount);

	public List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection)
		=> PickSome(() => _user.TradingPostTrash(cardPlayed, ps, k, cardSelection), cardSelection, 2, 2);

	public CardInstance UpgradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards)
		=> PickOne(() => _user.UpgradeTrash(cardPlayed, ps, k, cards), cards, required: true);

	public CardName WishingWellGuess(Card cardPlayed, PlayerState ps, Kingdom k, List<CardName> cardTypes)
		=> _user.WishingWellGuess(cardPlayed, ps, k, cardTypes);
	#endregion cards intrique

	#region helpers
	private static List<T> PickSome<T>(Func<List<T>> ask, IReadOnlyList<T> candidates, int min, int max)
	{
		// there is only one way to pick from candidates
		max = Math.Min(max, candidates.Count);
		min = Math.Min(min, max);
		if (max == 0)
		{
			return [];
		}
		if (min == max && candidates.Count == min)
		{
			return [.. candidates];
		}

		var result = ask();

		// validate response
		if (result.Count < min || result.Count > max)
		{
			throw new InvalidUserResponseException($"Expected selection count was between {min} and {max} but the actual was {result.Count}.");
		}

		if (result.Except(candidates).Any())
		{
			throw new InvalidUserResponseException($"There was an unexpected token in the response.");
		}

		if (result.Distinct().Count() != result.Count)
		{
			throw new InvalidUserResponseException($"Response tokens were not distinct.");
		}

		return result;
	}

	private static T PickOne<T>(Func<T> ask, IReadOnlyList<T> candidates, bool required)
	{
		// there is only one way to pick from candidates
		if (candidates.Count == 0)
		{
			return default;
		}

		if (required && candidates.Count == 1)
		{
			return candidates[0];
		}

		var result = ask();

		// validate response
		if (result is not null && !candidates.Contains(result))
		{
			throw new InvalidUserResponseException($"Response {result} was not among candidates.");
		}

		if (required && result is null)
		{
			throw new InvalidUserResponseException("Response is required but was null.");
		}

		return result;
	}

	private static List<T> Order<T>(Func<List<T>> ask, IReadOnlyList<T> candidates) where T : class
	{
		// there is only one way to order candidates
		if (candidates.Count <= 1)
		{
			return candidates.ToList();
		}

		var result = ask();

		// validate response
		if (result.Count != candidates.Count || result.Except(candidates).Any() || candidates.Except(result).Any())
		{
			throw new InvalidUserResponseException($"Response did not consist of right elements.");
		}
		return result;
	}
	#endregion helpers
}
