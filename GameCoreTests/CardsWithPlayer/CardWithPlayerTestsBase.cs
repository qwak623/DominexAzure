using GameCore.Cards;
using Moq;

namespace GameCore.CardWithPlayer.Tests;
public class CardWithPlayerTestsBase
{
	//private int nextCardId = 1000; // kept away from the real Kingdom's own id range to avoid collisions
	private Kingdom kingdom;
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
		var player = new Player(game, user);
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

	public Pile CreatePile(List<Card> cards)
	{
		//var kingdom = new Mock<Kingdom>();
		//kingdom.Setup(k => k.GetNextCardInstanceId()).Returns(() => nextCardId++); // todo this is static
		return new(cards, kingdom);
	}
}
