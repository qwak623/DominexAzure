#if false
using Moq;

namespace GameCore.Cards.Tests;
public class CardTestsBase
{
	public Kingdom MockKingdom(Card card)
	{
		return new Kingdom(new List<Card> { card }, 2); // todo should be mockable
	}

	public Mock<IPlayer> MockPlayer(Kingdom kingdom)
	{
		var player = new Mock<IPlayer>();

		var playerState = new PlayerState(playerStateObserver: null, "Tester")
		{
			Actions = 0,
			Coins = 0,
			Buys = 0,
			Hand = new Pile(),
		};
		player.Setup(p => p.PlayerState).Returns(playerState);
		player.Setup(p => p.Game.Kingdom).Returns(kingdom);

		return player;
	}

	public void AssertNumbers(int expectedActions, int expectedCoins, int expectedBuys, IPlayer player)
	{
		// consider Assert.Multiple(() =>
		Assert.AreEqual(expectedActions, player.PlayerState.Actions);
		Assert.AreEqual(expectedCoins, player.PlayerState.Coins);
		Assert.AreEqual(expectedBuys, player.PlayerState.Buys);
	}

	public Pile CreatePile(List<Card> cards)
	{
		var kingdom = new Mock<Kingdom>();
		kingdom.Setup(k => k.GetNextCardInstanceId()).Returns(() => nextCardId++); // todo this is static
		return new(cards, kingdom.Object);
	}
}

#endif
