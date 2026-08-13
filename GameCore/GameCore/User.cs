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

	public virtual void SetCanCelationTokenSource(CancellationTokenSource tokenSource) { }

	public abstract CardInstance PlayCard(IEnumerable<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null);

	public abstract CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public abstract CardInstance SelectOptionalCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase);

	public virtual IPlayerStateObserver GetPlayerStateObserver() => null;

	#region cards base
	public abstract List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);

	public abstract List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount);

	public abstract CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<CardInstance> cardSelection);
	public abstract bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k);

	public abstract bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p);

	public abstract CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	public abstract bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);

	public abstract CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	#endregion cards base

	#region cards intrique
	public abstract bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	public abstract CardInstance MasqueradePass(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	public abstract CardInstance MasqueradeTrash(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards);
	public abstract bool MiningVillageTrash(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c);
	public abstract bool MinionDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract bool NoblesChooseCards(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract bool TorturerChooseCurse(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> TorturerDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount);
	public abstract List<CardInstance> TradingPostTrash(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> SecretChamberDiscard(Card cardPlayed, PlayerState ps, Kingdom k);
	public abstract List<CardInstance> SecretChamberPutOnDeck(Card cardPlayed, PlayerState ps, Kingdom k, int count);
	#endregion cards intrique

	public override string ToString() => GetName();
}

public enum Phase { Action, Treasure, Buy, Gain, Reaction, Attack } // todo move to primitives
