using GameCore;
using GameCore.Cards;
using GameCore.Observers;

namespace Dominex.Services.Game;
public interface IHuman
{
	#region cards base
	Card BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k);
	List<Card> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	List<Card> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	string GetName();
	IPlayerStateObserver GetPlayerStateObserver();
	bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, Card c);
	List<Card> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount);
	Card MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<Card> cardSelection);
	Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null);
	Card RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);
	bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, Card c, Phase p);
	Card ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards);
	bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, Card c);
	Card ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards);
	#endregion cards base

	#region cards intrique
	bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	#endregion cards intrique
}