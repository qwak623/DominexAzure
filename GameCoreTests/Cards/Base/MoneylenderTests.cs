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
		// +3 Coins
		Assert.AreEqual(3, player.Object.PlayerState.Coins);

		// actions and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user isnt asked to choose whether to trash a copper - he doesnt have any
		player.Verify(p => p.User.MoneylenderTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player trashes nothing
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);
		#endregion
	}
}