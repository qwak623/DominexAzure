using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Intrique.Tests;

[TestClass]
public class BaronTests : CardTestsBase
{
	private readonly Card baron = Baron.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(baron));
	}

	[TestMethod]
	public void DiscardEstate()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { estate };
		player.Setup(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(true);
		#endregion

		#region act
		baron.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +4 Coins, +1 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(4, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to discard an estate
		player.Verify(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards the estate
		player.Verify(p => p.Discard(estate), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToDiscardEstate()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { estate };
		player.Setup(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		baron.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +1 Buy
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to discard an estate
		player.Verify(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards nothing
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player gains an estate
		player.Verify(p => p.Gain(CardType.Estate), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyEstate()
	{
		#region act
		baron.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +1 Buy
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user isn't asked to choose whether to discard an estate - he doesn't have any
		player.Verify(p => p.User.BaronDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player discards nothing
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player gains an estate
		player.Verify(p => p.Gain(CardType.Estate), Times.Once);
		#endregion
	}

	[TestMethod]
	[DataRow(2, true, true, 8, 2, 2)]
	[DataRow(2, true, false, 4, 2, 1)]
	[DataRow(2, false, true, 4, 2, 1)]
	[DataRow(2, false, false, 0, 2, 0)]
	[DataRow(1, true, true, 4, 1, 1)]
	[DataRow(1, false, true, 4, 2, 1)]
	[DataRow(1, false, false, 0, 2, 0)]
	[DataRow(0, true, true, 0, 0, 0)]
	public void ThroneRoomPlay(int estatesInHand, bool discardFirstTime, bool discardSecondTime, int coins, int askedToDiscard, int discardCount)
	{
		#region arrange
		player.Object.PlayerState.Hand = [baron, .. Enumerable.Repeat(estate, estatesInHand)];

		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == baron))).Returns(baron);
		player.SetupSequence(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		player.Setup(p => p.Discard(estate)).Callback(() => player.Object.PlayerState.Hand.Remove(estate));
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +given amount of coins
		Assert.AreEqual(coins, player.Object.PlayerState.Coins);

		// +0 Actions, +2 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to discard an estate given amout of times
		player.Verify(p => p.User.BaronDiscard(baron, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Exactly(askedToDiscard));

		// player discards the estate given amount of estates
		player.Verify(p => p.Discard(estate), Times.Exactly(discardCount));

		// player gains given amount of estates
		player.Verify(p => p.Gain(CardType.Estate), Times.Exactly(2 - discardCount));

		#endregion
	}
}