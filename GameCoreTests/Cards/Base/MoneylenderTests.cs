using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MoneylenderTests : CardTestsBase
{
	private readonly Card moneylender = Moneylender.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(moneylender));
	}

	[TestMethod]
	public void TrashCopper()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { copper };
		player.Setup(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(true);
		#endregion

		#region act
		moneylender.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +3 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(3, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to trash a copper
		player.Verify(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the copper
		player.Verify(p => p.Trash(copper), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToTrashCopper()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { copper };
		player.Setup(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		moneylender.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to trash a copper
		player.Verify(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes nothing
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyCopper()
	{
		#region act
		moneylender.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user isn't asked to choose whether to trash a copper - he doesn't have any
		player.Verify(p => p.User.MoneylenderTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player trashes nothing
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);
		#endregion
	}

	[TestMethod]
	[DataRow(2, true, true, 6, 2, 2)]
	[DataRow(2, true, false, 3, 2, 1)]
	[DataRow(2, false, true, 3, 2, 1)]
	[DataRow(2, false, false, 0, 2, 0)]
	[DataRow(1, true, true, 3, 1, 1)]
	[DataRow(1, false, true, 3, 2, 1)]
	[DataRow(1, false, false, 0, 2, 0)]
	[DataRow(0, true, true, 0, 0, 0)]
	public void ThroneRoomPlay(int coppersInHand, bool trashFirstTime, bool trashSecondTime, int coins, int askedToTrash, int trashCount)
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { moneylender };
		player.Object.PlayerState.Hand.AddRange(Enumerable.Repeat(copper, coppersInHand));

		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == moneylender))).Returns(moneylender);
		player.SetupSequence(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(trashFirstTime).Returns(trashSecondTime);
		player.Setup(p => p.Trash(copper)).Callback(() => player.Object.PlayerState.Hand.Remove(copper));
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +given amount of coins
		Assert.AreEqual(coins, player.Object.PlayerState.Coins);

		// +0 Actions, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to trash a copper given amout of times
		player.Verify(p => p.User.MoneylenderTrash(moneylender, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Exactly(askedToTrash));

		// player trashes the given amount of coppers
		player.Verify(p => p.Trash(copper), Times.Exactly(trashCount));
		#endregion
	}
}