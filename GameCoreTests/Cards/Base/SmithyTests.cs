#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class SmithyTests : CardTestsBase
{
	private readonly Card smithy = Smithy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(smithy));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		smithy.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player draws three cards
		player.Verify(p => p.Draw(3), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { smithy };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == smithy))).Returns(smithy);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// actions, coins and buys should not change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// (+3 Cards) * 2
		player.Verify(p => p.Draw(3), Times.Exactly(2));

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}
#endif
