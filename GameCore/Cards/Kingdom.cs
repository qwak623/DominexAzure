using System.Collections;
using GameCore.Observers;

namespace GameCore.Cards;
/// <summary>
/// Contains all cards for game.
/// For each game there should be unique instance.
/// </summary>
public class Kingdom : IEnumerable<KingdomPile>
{
	public int EmptyKingdomPilesCount => kingdomPiles.Count(p => p.Empty);
	private List<KingdomPile> kingdomPiles;
	private Dictionary<CardType, int> cardTypeToIndex = new Dictionary<CardType, int>();
	private int nextCardInstanceId = 0;

	public Kingdom(List<Card> cards, int playerCount, IKingdomObserver kingdomObserver = null)
	{
		kingdomPiles = cards.AddRequiredCards()
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
				return new KingdomPile(this, card, count);
			})
			.ToList();

		for (int i = 0; i < kingdomPiles.Count; i++)
		{
			cardTypeToIndex.Add(kingdomPiles[i].Type, i);
		}
		kingdomObserver?.Notify(this);
	}

	/// <summary>
	/// Returns new instance of KingdomWrapper with applied filters.
	/// </summary>
	/// <param name="price"></param>
	/// <param name="onlyTreasures"></param>
	/// <returns></returns>
	public KingdomWrapper GetWrapper(PlayerState ps, int price, bool onlyTreasures = false)
	{
		return new KingdomWrapper
		{
			Kingdom = this,
			MaxPrice = price,
			OnlyTreasures = onlyTreasures,
			PlayerState = ps,
		};
	}

	/// <summary>
	/// Returns pile with specified card (using dictionary).
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public KingdomPile GetPile(CardType type)
	{
		if (cardTypeToIndex.TryGetValue(type, out int index))
		{
			return kingdomPiles[index];
		}

		return null;
	}

	public KingdomPile this[int index] => kingdomPiles[index];

	public int Count => kingdomPiles.Count;

	public int GetNextCardInstanceId() => nextCardInstanceId++;

	public IEnumerator<KingdomPile> GetEnumerator()
	{
		foreach (var pile in kingdomPiles)
		{
			yield return pile;
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}