using GameCore;
using GameCore.Cards;

namespace AI.Provincial;

internal static class PlayAgenda
{
	public static float Score(this Card card, IEnumerable<Card> hand, PlayerState ps, Phase phase)
	{
		float result = 0;
		if (phase == Phase.Action && card.AddActions >= 1 && ps.Actions == 1)
		{
			result += 100;
		}

		switch (phase)
		{
			case Phase.Action:
				if (card.Name == CardName.Chapel)
				{
					return result + (hand.Where(c => c.Name == CardName.Curse).Count() * 3) + Data.GetPriorityList()[(int)card.Name];
				}

				if (card.Name == CardName.Library)
				{
					return result + -1.5f + (3 * (7 - ps.Hand.Count));
				}

				if (card.Name == CardName.Remodel)
				{
					return result + ((hand.Any(c => c.Name == CardName.Curse) ? 1 : 0) * 3) + Data.GetPriorityList()[(int)card.Name];
				}

				if (card.Name == CardName.Moneylender && ps.Hand.Any(c => c.Card.Name == CardName.Copper))
				{
					return -1;
				}

				if (card.Name == CardName.Mine && !(ps.Hand.Any(c => c.Card.Name == CardName.Copper) || ps.Hand.Any(c => c.Card.Name == CardName.Silver)))
				{
					return -1;
				}

				return Data.GetPriorityList()[(int)card.Name];
			case Phase.Attack:
				if (card.Name == CardName.Gold)
				{
					return 100;
				}

				if (card.Name == CardName.Silver)
				{
					return 50;
				}

				if (card.Name == CardName.Library)
				{
					return 49;
				}

				if (card.Name == CardName.Copper)
				{
					return 15;
				}

				goto case Phase.Action;
			default:
				break;
		}

		return -1;
	}
}