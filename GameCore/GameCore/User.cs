using GameCore.Cards;
using GameCore.Cards.Intrique;
using GameCore.Observers;

namespace GameCore;

/// <summary>
/// Interface for AI.
/// Every card, that requires decision has method here for easier implementation.
/// </summary>
public abstract class User : IUser
{
	public abstract string GetName();

	public virtual IPlayerStateObserver GetPlayerStateObserver() => null;

	public abstract CardInstance PlayCard(List<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);

	public abstract CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public abstract CardInstance SelectOptionalCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public virtual void SetCanCelationTokenSource(CancellationTokenSource tokenSource) { }

	#region cards base
	public abstract CardInstance ArtisanPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardInstance BanditTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract CardInstance HarbingerPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	public abstract List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	public abstract CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> PoacherDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	public abstract CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract List<CardInstance> SentryDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> SentryOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> SentryTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p);
	public abstract CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	public abstract CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract bool VassalPlay(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance card);
	#endregion cards base

	#region cards intrique
	public abstract bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CourtierBenefit> CourtierChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<CourtierBenefit> availableBenefits);
	public abstract CardInstance CourtierReveal(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> DiplomatDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	public abstract CardInstance LurkerChooseCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardInstance LurkerChooseCardToTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract bool LurkerTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> MillChooseCardsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards, int count);
	public abstract bool MillWantsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	public abstract bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract bool NoblesChooseCards(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> PatrolOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<PawnBenefit> PawnChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<PawnBenefit> availableBenefits);
	public abstract CardInstance ReplaceTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> ScoutOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	public abstract CardInstance SecretPassageChooseCard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract StewardBenefit StewardChooseBenefit(Card cardPlayed, PlayerState ps, Kingdom k, List<StewardBenefit> allBenefits);
	public abstract List<CardInstance> StewardChooseCardsToTrash(Card cardPlayed, PlayerState ps, Kingdom k, int count, List<CardInstance> cards);
	public abstract bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	public abstract List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	public abstract CardInstance UpgradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	public abstract CardName WishingWellGuess(Card cardPlayed, PlayerState ps, Kingdom k, List<CardName> cardTypes);
	#endregion cards intrique

	public override string ToString() => GetName();
}

public enum Phase { Action, Treasure, Buy, Gain, Reaction, Attack } // todo move to primitives
