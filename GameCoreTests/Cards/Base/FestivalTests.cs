#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class FestivalTests : CardTestsBase
{
	private readonly Card festival = Festival.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(festival));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		festival.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +2 Actions, +2 Coins, +1 Buy
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { festival };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == festival))).Returns(festival);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// (+2 Actions, +2 Coins, +1 Buy) * 2
		Assert.AreEqual(4, player.Object.PlayerState.Actions);
		Assert.AreEqual(4, player.Object.PlayerState.Coins);
		Assert.AreEqual(2, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}
#endif
