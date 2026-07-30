using GameCore;
using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.Intrique;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCoreTests.Cards.Intrique;

[TestClass]
public class ConspiratorTests : CardTestsBase
{
	private readonly Card conspirator = Conspirator.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(conspirator));
	}

	[TestMethod]
	public void PlayAsFirstCard()
	{
		#region arrange
		player.Object.PlayerState.ActionsPlayed = [conspirator];
		player.Object.PlayerState.CardsPlayed = [conspirator];
		#endregion

		#region act
		conspirator.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +2 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void PlayAsSecondCard()
	{
		#region arrange
		player.Object.PlayerState.ActionsPlayed = [conspirator, conspirator];
		player.Object.PlayerState.CardsPlayed = [conspirator, conspirator];
		#endregion

		#region act
		conspirator.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +2 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);
		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void PlayAsThirdCard()
	{
		#region arrange
		player.Object.PlayerState.ActionsPlayed = [conspirator, conspirator, conspirator];
		player.Object.PlayerState.CardsPlayed = [conspirator, conspirator, conspirator];
		#endregion

		#region act
		conspirator.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action, +2 Coins, +0 Buys
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +1 Card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsFirst()
	{
		#region arrange
		player.Object.PlayerState.ActionsPlayed = [throneRoom];
		player.Object.PlayerState.CardsPlayed = [throneRoom];
		player.Object.PlayerState.Hand = [conspirator];
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == conspirator))).Returns(conspirator);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(4, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +1 Action
		Assert.AreEqual(1, player.Object.PlayerState.Actions);

		// +1 Cards
		player.Verify(p => p.Draw(1), Times.Once);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsSecond()
	{
		#region arrange
		player.Object.PlayerState.ActionsPlayed = [conspirator, throneRoom];
		player.Object.PlayerState.CardsPlayed = [conspirator, throneRoom];

		player.Object.PlayerState.Hand = [conspirator];
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == conspirator))).Returns(conspirator);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// (+1 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(4, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +2 Cards
		player.Verify(p => p.Draw(1), Times.Exactly(2));

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}