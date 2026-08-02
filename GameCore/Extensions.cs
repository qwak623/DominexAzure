using GameCore.Cards;
using GameCore.Observers;
using Utils;

namespace GameCore;

public static class Extensions
{
	public static void Shuffle<T>(this List<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = ThreadSafeRandom.Next(n + 1);
			(list[k], list[n]) = (list[n], list[k]);
		}
	}

	/// <summary>
	/// Returns prepared piles for new game based on kingdomCards and player count.
	/// </summary>
	/// <param name="cards"></param>
	/// <param name="players"></param>
	/// <returns></returns>
	public static Kingdom GetKingdom(this List<Card> cards, int players, IKingdomObserver kingdomObserver = null)
	{
		// this should be correct, since list of cards wont change at this point
		return new Kingdom(cards, players, kingdomObserver);
	}

	public static List<Card> AddRequiredCards(this IEnumerable<Card> cards)
	{
		return cards.Concat(PresetGames.VictoryAndTreasures())
			.Concat(cards.Select(c => c.RequiredCards).Where(c => c is not null).Distinct()).ToList();
	}

	public static bool Contains(this IEnumerable<Card> cards, CardType type)
	{
		foreach (var card in cards)
		{
			if (card.Type == type)
			{
				return true;
			}
		}

		return false;
	}

	public static List<Card> ToCardList(this string id)
	{
		try
		{
			return id.Split('_').Select(a => Card.Get((CardType)int.Parse(a))).ToList();
		}
		catch
		{
			return null;
		}
	}

	public static string ToId(this IEnumerable<Card> cardList)
	{
		return cardList.OrderBy(p => p.Type).Select(p => ((int)p.Type).ToString()).Aggregate((a, b) => a + "_" + b);
	}
}
