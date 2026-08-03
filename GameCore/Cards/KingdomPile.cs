namespace GameCore.Cards;

public class KingdomPile
{
	private readonly Pile pile;
	private CardInstance last;

	public int Count => pile.Count;
	public bool Empty => pile.Count == 0;
	public CardType Type { get; init; }
	public string Name { get; init; }
	public Card CardToDisplay => pile.Count != 0 ? CardInstance.Card : last.Card;
	public CardInstance CardInstance => pile.Count != 0 ? pile[^1] : null;

	public KingdomPile(Kingdom kingdom, Card card, int count = 1)
	{
		pile = new Pile(card, count, kingdom);

		last = pile[0];
		Type = card.Type;
		Name = card.Name;
	}

	// TODO this needs a player parameter to get the correct price (bridge, etc.)
	public override string ToString() => $"{Name} ${CardToDisplay.GetPrice(null)} ({Count})";
}
