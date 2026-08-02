using System.Collections;

namespace GameCore.Cards;

public sealed class Pile : IReadOnlyList<CardInstance>
{
	private readonly List<CardInstance> cards;

	public int Count => cards.Count;

	public CardInstance this[int index] => cards[index];

	public IEnumerator<CardInstance> GetEnumerator() => cards.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public Pile()
	{
		cards = [];
	}

	public Pile(Card card, int count, Kingdom kingdom)
	{
		cards = [.. Enumerable.Repeat(card, count).Select(c => new CardInstance(c, this, kingdom.GetNextCardInstanceId()))];
	}

	public Pile(List<Card> initCards, Kingdom kingdom)
	{
		cards = initCards.Select(c => new CardInstance(c, this, kingdom.GetNextCardInstanceId())).ToList();
	}

	public void Move(CardInstance cardInstance)
	{
		cards.Add(cardInstance);
		cardInstance.Pile.cards.Remove(cardInstance);
		cardInstance.Pile = this;
	}

	public void MoveRange(IList<CardInstance> cardInstances)
	{
		cards.AddRange(cardInstances);
		foreach (var cardInstance in cardInstances)
		{
			cardInstance.Pile.cards.Remove(cardInstance);
			cardInstance.Pile = this;
		}
	}

	public void MoveAll(Pile pile)
	{
		cards.AddRange(pile);
		foreach (var cardInstance in pile)
		{
			cardInstance.Pile = this;
		}
		pile.cards.Clear();
	}

	public void Shuffle()
	{
		cards.Shuffle();
	}
}
