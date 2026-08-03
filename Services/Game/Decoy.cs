using GameCore.Cards;
using GameCore;
using GameCore.Cards.Base;

namespace Dominex.Services.Game;
public class Decoy : User
{
	#region cards base
	public override CardInstance BureaucratPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		if (ps.Hand.Count == 0)
		{
			throw new InvalidOperationException("No cards in hand to put on top of the deck.");
		}
		return ps.Hand[0];
	}

	public override List<CardInstance> CellarDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return [];
	}

	public override bool ChancellorDiscard(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return false;
	}

	public override List<CardInstance> ChapelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return [];
	}

	public override string GetName() => "TODO NAME 2";

	public override bool LibrarySkip(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
	{
		return false;
	}

	public override List<CardInstance> MilitiaDiscard(Card cardPlayed, PlayerState ps, Kingdom k, int discardCount)
	{
		return [.. ps.Hand.Take(2)];
	}

	public override CardInstance MineTrash(Card cardPlayed, PlayerState ps, Kingdom k, IList<CardInstance> cardSelection)
	{
		return null;
	}

	public override bool MoneylenderTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return true;
	}

	public override CardInstance PlayCard(IEnumerable<CardInstance> cards, PlayerState ps, Kingdom k, Phase phase, Card attackCard = null)
	{
		return null;
	}

	public override CardInstance RemodelTrash(Card cardPlayed, PlayerState ps, Kingdom k)
	{
		return null;
	}

	public override CardInstance SelectCardToGain(KingdomWrapper wrapper, PlayerState ps, Kingdom k, Phase phase)
	{
		return wrapper.AvailableCards.First();
	}

	public override bool SpyDiscard(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c, Phase p)
	{
		return false;
	}

	public override CardInstance ThiefChoose(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		return cards.First();
	}

	public override bool ThiefSteal(Card cardPlayed, PlayerState ps, Kingdom k, CardInstance c)
	{
		return true;
	}

	public override CardInstance ThroneRoomPlay(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
	{
		return null;
	}
	#endregion cards base

	#region cards intrique
	public override bool BaronDiscard(Card cardPlayed, PlayerState ps, Kingdom k) => true;

	public override CardInstance CourtyardPutOnTop(Card cardPlayed, PlayerState ps, Kingdom k, IEnumerable<CardInstance> cards)
		=> cards.FirstOrDefault();
	#endregion cards intrique
}