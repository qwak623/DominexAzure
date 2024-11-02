using GameCore.Cards;
using GameCore.Observers;
using System.Collections.Generic;
using System.Threading;

namespace GameCore;

/// <summary>
/// Interface for AI.
/// Every card, that requires decision has method here for easier implementation.
/// </summary>
public abstract class User
{
	public abstract string GetName();

	public virtual void SetCanCelationTokenSource(CancellationTokenSource tokenSource) { }

	public abstract Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);

	public abstract Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public virtual IPlayerStateObserver GetPlayerStateObserver() => null;

	#region cards base
	public abstract List<Card> CellarDiscard(PlayerState ps, Kingdom k);

	public abstract Card BureaucratDiscard(PlayerState ps, Kingdom k);

	public abstract bool ChancellorDiscard(PlayerState ps, Kingdom k);

	public abstract List<Card> ChapelTrash(PlayerState ps, Kingdom k);

	public abstract bool LibrarySkip(PlayerState ps, Kingdom k, Card c);

	public abstract List<Card> MilitiaDiscard(PlayerState ps, Kingdom k, int discardCount);

	public abstract Card MineTrash(PlayerState ps, Kingdom k);

	public abstract Card RemodelTrash(PlayerState ps, Kingdom k);

	public abstract bool SpyDiscard(PlayerState ps, Kingdom k, Card c, Phase p);

	public abstract Card ThiefChoose(PlayerState ps, Kingdom k, IEnumerable<Card> cards);

	public abstract bool ThiefSteal(PlayerState ps, Kingdom k, Card c);

	public abstract Card ThroneRoomPlay(PlayerState ps, Kingdom k, IEnumerable<Card> cards);
	#endregion

	public override string ToString() => GetName();
}

public enum Phase { Action, Treasure, Buy, Gain, Reaction, Attack } // todo move to primitives
