using GameCore;
using GameCore.Cards;
using GameCore.Observers;

namespace Dominex.Services.Game;
public interface IHuman
{
	Card BureaucratPutOnTop(PlayerState ps, Kingdom k);
	List<Card> CellarDiscard(PlayerState ps, Kingdom k);
	bool ChancellorDiscard(PlayerState ps, Kingdom k);
	List<Card> ChapelTrash(PlayerState ps, Kingdom k);
	string GetName();
	IPlayerStateObserver GetPlayerStateObserver();
	bool LibrarySkip(PlayerState ps, Kingdom k, Card c);
	List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount);
	Card MineTrash(PlayerState ps, Kingdom k);
	Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card attackingCard = null);
	Card RemodelTrash(PlayerState ps, Kingdom k);
	Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);
	bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p);
	Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards);
	bool ThiefSteal(PlayerState ps, Kingdom k, Card c);
	Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards);
}