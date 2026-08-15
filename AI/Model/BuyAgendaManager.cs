using System.Collections;
using GameCore.Cards;
using GameCore.Observers;

namespace AI.Model;
public abstract class BuyAgendaManager : IEnumerable<BuyAgenda>
{
	/// <summary>
	/// Loads specified agenda.
	/// </summary>
	/// <param name="cards"></param>
	/// <returns></returns>
	public abstract BuyAgenda Load(IEnumerable<Card> cards);
	public BuyAgenda Load(IEnumerable<int> cards) => Load(cards.Select(c => Card.Get((CardType)c)));

	/// <summary>
	/// Finds best agenda on specified kingdom. SimpleManager just redirects call to Load method.
	/// </summary>
	/// <param name="cards"></param>
	/// <param name="logger"></param>
	/// <returns></returns>
	public abstract BuyAgenda LoadBest(List<Card> cards, IGameLogger? logger = null);

	/// <summary>
	/// Saves agenda.
	/// </summary>
	/// <param name="cards"></param>
	/// <param name="agenda"></param>
	public abstract void Save(IEnumerable<Card> cards, BuyAgenda agenda);
	public void Save(IEnumerable<int> cards, BuyAgenda agenda) => Save(cards.Select(c => Card.Get((CardType)c)), agenda);

	public abstract IEnumerator<BuyAgenda> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
