using GameCore.Cards;
using GameCore;

namespace Dominex.Services.Game;
public class Decoy : User
{
	public override Card BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return ps.Hand.FirstOrDefault();
	}

	public override List<Card> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return new();
	}

	public override bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return false;
	}

	public override List<Card> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return new();
	}

	public override string GetName() => "TODO NAME 2";

	public override bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, Card c)
	{
		return false;
	}

	public override List<Card> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		return ps.Hand.Take(2).ToList();
	}

	public override Card MineTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return null;
	}

	public override Card PlayCard(IEnumerable<Card> cards, PlayerState ps, Kingdom k, Phase phase, Card card = null)
	{
		return null;
	}

	public override Card RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return null;
	}

	public override Card SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		return wrapper.AvailableCards.First();
	}

	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, Card c, Phase p)
	{
		return false;
	}

	public override Card ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		return null;
	}

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, Card c)
	{
		return true;
	}

	public override Card ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<Card> cards)
	{
		return null;
	}
}