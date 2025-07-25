using System.Collections;
using GameCore.Observers;

namespace GameCore.Cards;
/// <summary>
/// Contains all cards for game.
/// For each game there should be unique instance.
/// </summary>
public class Kingdom : IEnumerable<Pile>
{
	public int EmptyPilesCount;
	private List<Pile> piles;
	private Dictionary<CardType, int> cardTypeToIndex = new Dictionary<CardType, int>();

	public Kingdom(List<Card> cards, int playerCount, IKingdomObserver kingdomObserver = null)
	{
		piles = cards.AddRequiredCards()
			.Select(card =>
			{
				int count = 10;
				if (card.Type == CardType.Curse)
				{
					count = (playerCount - 1) * 10;
				}
				else if (card.IsVictory)
				{
					count = playerCount == 2 ? 8 : 12;
				}
				else if (card.Type == CardType.Copper)
				{
					count = 60;
				}
				else if (card.Type == CardType.Silver)
				{
					count = 40;
				}
				else if (card.Type == CardType.Gold)
				{
					count = 30;
				}
				return new Pile(card, count, () => kingdomObserver?.Notify(this));
			})
			.ToList();

		for (int i = 0; i < piles.Count; i++)
		{
			cardTypeToIndex.Add(piles[i].Card.Type, i);
		}
		kingdomObserver?.Notify(this);
	}

	/// <summary>
	/// Returns new instance of KingdomWrapper with applied filters.
	/// </summary>
	/// <param name="price"></param>
	/// <param name="onlyTreasures"></param>
	/// <returns></returns>
	public KingdomWrapper GetWrapper(int price, bool onlyTreasures = false)
	{
		return new KingdomWrapper
		{
			Kingdom = this,
			Price = price,
			OnlyTreasures = onlyTreasures
		};
	}

	/// <summary>
	/// Returns pile with specified card (using dictionary).
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Pile GetPile(CardType type)
	{
		if (cardTypeToIndex.TryGetValue(type, out int index))
		{
			return piles[index];
		}

		return null;
	}

	public Pile this[int index] => piles[index];

	public int Count => piles.Count;

	public IEnumerator<Pile> GetEnumerator()
	{
		foreach (var pile in piles)
		{
			yield return pile;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}