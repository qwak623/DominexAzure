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
	private readonly List<KingdomPile> kingdomPiles;
	private readonly Dictionary<CardName, int> cardTypeToIndex = [];
	private int nextCardInstanceId = 0;

	public Kingdom(List<Card> cards, int playerCount, bool addColonyAndPlatinum = false, IKingdomObserver kingdomObserver = null)
	{
		kingdomPiles = cards.AddRequiredCards(addColonyAndPlatinum)
			.Select(card => new KingdomPile(this, card, card.GetCountInKingdomPile(playerCount)))
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
	public KingdomPile GetPile(CardName type)
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