using GameCore.Cards;
using GameCore.Observers;

namespace GameCore;
public interface IUser
{
	string GetName();
	IPlayerStateObserver GetPlayerStateObserver();
	CardInstance PlayCard(IEnumerable<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);
	CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);
	void SetCanCelationTokenSource(CancellationTokenSource tokenSource);

	#region cards base
	CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount);
	CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<CardInstance> cardSelection);
	bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p);
	CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	#endregion cards base

	#region cards intrique
	bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	#endregion cards intrique

	string ToString();
}