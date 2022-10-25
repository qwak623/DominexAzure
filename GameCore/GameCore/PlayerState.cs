using GameCore.Cards;
using System.Collections.Generic;

namespace GameCore;

/// <summary>
/// Whatever what window needs to show (but it is not important for functionality).
/// </summary>
public class PlayerState
{
	public string Name;
	public int Actions;
	public int Buys;
	public int Coins;

	public List<Card> DrawPile = new();
	public List<Card> DiscardPile = new();
	public List<Card> Hand = new();
	public List<Card> PlayedCards = new(10);
}
