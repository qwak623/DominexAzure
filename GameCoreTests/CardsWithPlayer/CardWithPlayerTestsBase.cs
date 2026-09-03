using GameCore.Cards;
using GameCore.GameCore;
using Moq;

namespace GameCore.CardWithPlayer.Tests;
public class CardWithPlayerTestsBase
{
	//private int nextCardId = 1000; // kept away from the real Kingdom's own id range to avoid collisions
	private Kingdom kingdom;
	private readonly Pile removedCards = new();
	public Mock<IGame> MockGame(Card card)
	{
		return MockGame([card]);
	}

	public Mock<IGame> MockGame(List<Card> cards)
	{
		kingdom = new Kingdom(cards, 2); // todo should be mockable

		var game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);
		game.Setup(g => g.Trash).Returns(new Pile());
		return game;
	}

	public Player CreatePlayer(IGame game, IUser user)
	{
		var userProxy = new UserProxy(user);
		var player = new Player(game, userProxy);
		player.PlayerState.Actions = 1;
		player.PlayerState.Buys = 0;
		player.PlayerState.Coins = 0;
		player.PlayerState.Hand = new Pile();
		player.PlayerState.CardsPlayed = new Pile();
		player.PlayerState.ActionsPlayed = [];
		player.PlayerState.DrawPile = new Pile();
		player.PlayerState.DiscardPile = new Pile();
		return player;
	}

	public void AssertNumbers(int expectedActions, int expectedCoins, int expectedBuys, Player player)
	{
		// consider Assert.Multiple(() =>
		Assert.AreEqual(expectedActions, player.PlayerState.Actions);
		Assert.AreEqual(expectedCoins, player.PlayerState.Coins);
		Assert.AreEqual(expectedBuys, player.PlayerState.Buys);
	}

	public void AssertPile(List<Card> expected, Pile actual)
	{
		Assert.AreSequenceEqual(expected, actual.Select(c => c.Card).ToList(), SequenceOrder.InAnyOrder);
	}

	public void AssertPile(List<Card> expected, List<Card> actual)
	{
		Assert.AreSequenceEqual(expected, actual, SequenceOrder.InAnyOrder);
	}

	/// <summary>
	/// Drains a kingdom pile down to <paramref name="count"/> cards (default 0).
	/// Since <see cref="UserProxy"/> no longer lets a mocked user express "I gain nothing"
	/// by returning null from a required selection, tests simulate "nothing available to
	/// gain / buy" by emptying the relevant piles instead.
	/// </summary>
	public void SetKingdomPileCount(CardName card, int count = 0)
	{
		var pile = kingdom.GetPile(card)
			?? throw new InvalidOperationException($"Kingdom has no {card} pile.");
		while (pile.Count > count)
		{
			removedCards.Move(pile.CardInstance);
		}
	}

	/// <summary>
	/// Empties every kingdom pile, so nothing can be gained or bought.
	/// </summary>
	public void EmptyKingdom(params CardName[] except)
	{
		foreach (var pile in kingdom)
		{
			if (except.Contains(pile.Type))
			{
				continue;
			}
			while (pile.Count > 0)
			{
				removedCards.Move(pile.CardInstance);
			}
		}
	}

	public Pile CreatePile(List<Card> cards)
	{
		//var kingdom = new Mock<Kingdom>();
		//kingdom.Setup(k => k.GetNextCardInstanceId()).Returns(() => nextCardId++); // todo this is static
		return new(cards, kingdom);
	}
}
