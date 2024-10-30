using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Cards;

// todo jb revision
public class KingdomWrapper
{
	public Kingdom kingdom;
	public int price;
	public bool onlyTreasures;

	/// <summary>
	/// Returns specified card, if it is available.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Card GetCard(CardType type)
	{
		var pile = kingdom.GetPile(type);
		if (pile != null && isAvailable(pile))
		{
			return kingdom.GetPile(type).Card;
		}

		return null;
	}

	/// <summary>
	/// Returns all available cards.
	/// </summary>
	public IEnumerable<Card> AvailableCards =>
		kingdom.Where(p => isAvailable(p))
		.Select(p => p.Card);

	bool isAvailable(Pile pile) => pile.Count > 0 && pile.Price <= price && (onlyTreasures && pile.Card.IsTreasure || !onlyTreasures);
}
