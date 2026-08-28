using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.Intrique;
using GameCore.Observers;

namespace GameCore;
public interface IUser
{
	string GetName();
	IPlayerStateObserver GetPlayerStateObserver();
	CardInstance PlayCard(List<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);
	CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);
	CardInstance SelectOptionalCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);
	void SetCanCelationTokenSource(CancellationTokenSource tokenSource);

	#region cards base
	CardInstance ArtisanPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance BanditTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	CardInstance HarbingerPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> PoacherDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	List<CardInstance> SentryDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> SentryOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> SentryTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p);
	CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	#endregion cards base

	#region cards intrique
	bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CourtierBenefit> CourtierChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<CourtierBenefit> availableBenefits);
	CardInstance CourtierReveal(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> DiplomatDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	CardInstance LurkerChooseCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance LurkerChooseCardToTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool LurkerTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> MillChooseCardsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards, int count);
	bool MillWantsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	bool NoblesChooseCards(Card cardPlayed, PlayerState playerState, Kingdom kingdom);
	List<CardInstance> PatrolOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<PawnBenefit> PawnChooseBenefits(Card cardPlayed, PlayerState ps, Kingdom k, int benefitCount, List<PawnBenefit> availableBenefits);
	CardInstance ReplaceTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> ScoutOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	CardInstance SecretPassageChooseCard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	StewardBenefit StewardChooseBenefit(Card cardPlayed, PlayerState ps, Kingdom k, List<StewardBenefit> allBenefits);
	List<CardInstance> StewardChooseCardsToTrash(Card cardPlayed, PlayerState ps, Kingdom k, int count, List<CardInstance> cards);
	bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	CardInstance UpgradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardName WishingWellGuess(Card cardPlayed, PlayerState ps, Kingdom k, List<CardName> cardTypes);
	#endregion cards intrique

	string ToString();
}
