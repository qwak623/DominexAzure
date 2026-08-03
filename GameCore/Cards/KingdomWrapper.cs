namespace GameCore.Cards;

// todo jb revision
// asi bude lepsi sloucit s kingdomem
// nebo z toho udelat readonly kingdom
public class KingdomWrapper
{
	public Kingdom Kingdom;
	public int Price { get; init; }
	public bool OnlyTreasures { get; init; }
	public PlayerState PlayerState { get; init; }

	/// <summary>
	/// Returns specified card, if it is available.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public CardInstance GetCard(CardType type)
	{
		var pile = Kingdom.GetPile(type);
		if (pile is not null && IsAvailable(pile))
		{
			return pile.CardInstance;
		}

		return null;
	}

	/// <summary>
	/// Returns all available cards.
	/// </summary>
	public IEnumerable<CardInstance> AvailableCards =>
		Kingdom.Where(IsAvailable)
		.Select(p => p.CardInstance);

	private bool IsAvailable(KingdomPile kp) =>
		kp.Count > 0 && kp.CardInstance.Card.GetPrice(PlayerState) <= Price
		&& ((OnlyTreasures && kp.CardInstance.IsTreasure) || !OnlyTreasures);
}
