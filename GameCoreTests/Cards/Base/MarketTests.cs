using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MarketTests : CardTestsBase
{
	private readonly Card market = Market.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(market));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		market.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action, +1 Coin, +1 Buy
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(1, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// player draws one card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}
}