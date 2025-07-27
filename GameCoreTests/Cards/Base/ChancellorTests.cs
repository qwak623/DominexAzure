using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class ChancellorTests : CardTestsBase
{
	private readonly Card chancellor = Chancellor.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(chancellor));
	}

	[TestMethod]
	public void DiscardDrawPile()
	{
		#region arrange
		player.Setup(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(true);
		#endregion

		#region act
		chancellor.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, player.Object.PlayerState.Coins);

		// actions and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to discard his draw pile
		player.Verify(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards his draw pile
		player.Verify(p => p.DiscardDrawPile(), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontDiscardDrawPile()
	{
		#region arrange
		player.Setup(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		chancellor.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, player.Object.PlayerState.Coins);

		// actions and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose whether to discard his draw pile
		player.Verify(p => p.User.ChancellorDiscard(chancellor, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player never discards his draw pile
		player.Verify(p => p.DiscardDrawPile(), Times.Never);
		#endregion
	}
}