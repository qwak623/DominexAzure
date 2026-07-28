using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Observers;

namespace GameCore;

/// <summary>
/// Interface for AI.
/// Every card, that requires decision has method here for easier implementation.
/// </summary>
public abstract class User : IUser
{
	public abstract string GetName();

	public virtual void SetCanCelationTokenSource(CancellationTokenSource tokenSource) { }

	public abstract Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);

	public abstract Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public virtual IPlayerStateObserver GetPlayerStateObserver() => null;

	#region cards base
	public abstract List<Card> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract Card BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract List<Card> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, Card c);

	public abstract List<Card> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount);

	public abstract Card MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<Card> cardSelection);

	public abstract bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract Card RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, Card c, Phase p);

	public abstract Card ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards);

	public abstract bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, Card c);

	public abstract Card ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards);
	#endregion cards intrique

	#region cards intrique
	public abstract bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	#endregion cards intrique

	public override string ToString() => GetName();
}

public enum Phase { Action, Treasure, Buy, Gain, Reaction, Attack } // todo move to primitives
