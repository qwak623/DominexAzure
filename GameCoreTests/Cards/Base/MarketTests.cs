#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MarketTests : CardTestsBase
{
	private readonly Card market = Market.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

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
		market.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +1 Action, +1 Coin, +1 Buy
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(1, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// +1 Card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { market };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == market))).Returns(market);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// (+1 Action, +1 Coin, +1 Buy) * 2
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(2, player.Object.PlayerState.Buys);

		// (+1 Card) * 2
		player.Verify(p => p.Draw(1), Times.Exactly(2));

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}
#endif
