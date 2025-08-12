using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class VillageTests : CardTestsBase
{
	private readonly Card village = Village.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(village));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		village.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +2 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +1 Card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { village };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == village))).Returns(village);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// (+2 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(4, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// (+1 Card) * 2
		player.Verify(p => p.Draw(1), Times.Exactly(2));

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}