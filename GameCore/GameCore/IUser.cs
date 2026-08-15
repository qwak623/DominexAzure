using GameCore.Cards;
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
	CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p);
	CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	#endregion cards base

	#region cards intrique
	bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int discardCount);
	List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	bool NoblesChooseCards(Card cardPlayed, PlayerState playerState, Kingdom kingdom);
	List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection);
	List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	List<CardInstance> ScoutOrderCards(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	List<CardInstance> DiplomatDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cardSelection, int count);
	bool LurkerTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance LurkerChooseCardToTrash(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	CardInstance LurkerChooseCardToGain(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards);
	bool MillWantsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> MillChooseCardsToDiscard(Card cardPlayed, PlayerState ps, Kingdom k, List<CardInstance> cards, int count);
	#endregion cards intrique

	string ToString();
}
