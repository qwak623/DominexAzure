namespace GameCore.Cards;

// todo jb revision
// asi bude lepsi sloucit s kingdomem
// nebo z toho udelat readonly kingdom
public class KingdomWrapper
{
	public Kingdom Kingdom;
	public int Price { get; init; }
	public bool OnlyTreasures { get; init; }

	/// <summary>
	/// Returns specified card, if it is available.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Card GetCard(CardType type)
	{
		var pile = Kingdom.GetPile(type);
		if (pile != null && IsAvailable(pile))
		{
			return Kingdom.GetPile(type).Card;
		}

		return null;
	}

	/// <summary>
	/// Returns all available cards.
	/// </summary>
	public IEnumerable<Card> AvailableCards =>
		Kingdom.Where(IsAvailable)
		.Select(p => p.Card);

	private bool IsAvailable(Pile pile) => pile.Count > 0 && pile.Price <= Price && (OnlyTreasures && pile.Card.IsTreasure || !OnlyTreasures);
}
