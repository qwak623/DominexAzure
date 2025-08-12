namespace GameCore.Cards;

public class Pile
{
	// TODO zrefaktorovat, takhle bereme card místo top
	private readonly Stack<Card> cards;
	private readonly Action onGain;
	private Card top;

	public int Count => cards.Count;
	public bool Empty => !cards.Any();
	public CardType Type { get; init; }
	public string Name { get; init; }
	public int Price { get; init; }
	public Card Card => cards.Any() ? top : null;

	public Card GainCard()
	{
		if (Empty)
		{
			return null;
		}

		var card = cards.Pop();
		onGain?.Invoke();
		top = Empty ? null : cards.Peek();
		return card;
	}

	public Pile(Card card, int count = 1, Action onGain = null)
	{
		cards = new Stack<Card>();
		for (int i = 0; i < count; i++)
		{
			cards.Push(card);
		}

		top = cards.Peek();
		Type = card.Type;
		Name = card.Name;
		Price = card.Price;
		this.onGain = onGain;
	}

	public override string ToString() => $"{Name} ${Price} ({Count})";
}
