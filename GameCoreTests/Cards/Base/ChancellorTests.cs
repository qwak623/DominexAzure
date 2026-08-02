#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class ChancellorTests : CardTestsBase
{
	private readonly Card chancellor = Chancellor.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(chancellor));
	}

	[TestMethod]
	[DataRow(true, 1)]
	[DataRow(false, 0)]
	public void DiscardDrawPile(bool discard, int numberOfDiscards)
	{
		#region arrange
		player.Setup(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(discard);
		#endregion

		#region act
		chancellor.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +2 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to discard his draw pile
		player.Verify(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards his draw pile
		player.Verify(p => p.DiscardDrawPile(), Times.Exactly(numberOfDiscards));
		#endregion
	}

	[TestMethod]
	[DataRow(true, true, 2, DisplayName = "DiscardsBothTimes")]
	[DataRow(true, false, 1, DisplayName = "OnlyDiscardsForTheFirstTime")]
	[DataRow(false, true, 1, DisplayName = "OnlyDiscardsForTheSecondTime")]
	[DataRow(false, false, 0, DisplayName = "DoesntDiscard")]
	public void ThroneRoomPlay(bool discardFirstTime, bool discardSecondTime, int numberOfDiscards)
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { chancellor };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == chancellor))).Returns(chancellor);
		player.SetupSequence(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(4, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to discard his draw pile two times
		player.Verify(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Exactly(2));

		// player discards his draw pile one time
		player.Verify(p => p.DiscardDrawPile(), Times.Exactly(numberOfDiscards));
		#endregion
	}
}
#endif
