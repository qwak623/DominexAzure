using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class CouncilRoomTests : CardTestsBase
{
	private readonly Card councilRoom = CouncilRoom.Get();

	private Kingdom kingdom;
	private Mock<IPlayer> player;
	private Mock<IPlayer> player2;
	private Mock<IPlayer> player3;
	private Mock<IPlayer> player4;

	[TestInitialize]
	public void Init()
	{
		kingdom = MockKingdom(councilRoom);
		player = MockPlayer(kingdom);
		player2 = MockPlayer(kingdom);
		player3 = MockPlayer(kingdom);
		player4 = MockPlayer(kingdom);
		var players = new List<IPlayer> { player2.Object, player.Object, player3.Object, player4.Object };
		player.Setup(p => p.Game.Players).Returns(players);
	}

	[TestMethod]
	public void OtherPlayersDrawCard()
	{
		#region act
		councilRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Buy
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// Actions and coins don't change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);

		// player draws 4 cards
		player.Verify(p => p.Draw(4), Times.Once);

		// all the other players draw one card
		player2.Verify(p => p.Draw(1), Times.Once);
		player3.Verify(p => p.Draw(1), Times.Once);
		player4.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}
}