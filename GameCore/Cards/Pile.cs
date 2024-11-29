namespace GameCore.Cards;
public class Pile

{
	// TODO zrefaktorovat, takhle bereme card místo top
	private readonly Stack<Card> cards;
	private readonly Action onGain;
	private Card top;

	public int Count => cards.Count;
	public bool Empty => cards.Count == 0;
	public CardType Type => top.Type;
	public string Name => top.Name;
	public int Price => top.Price;
	public Card Card => cards.Count > 0 ? top : null;

	public Card GainCard()
	{
		if (Empty)
		{
			return null;
		}

		top = cards.Pop();
		onGain?.Invoke();
		return top;
	}

	public Pile(Card card, int count = 1, Action onGain = null)
	{
		cards = new Stack<Card>();
		for (int i = 0; i < count; i++)
		{
			cards.Push(card);
		}

		top = cards.Peek();
		this.onGain = onGain;
	}

	public override string ToString() => $"{Name} ${Price} ({Count})";
}
