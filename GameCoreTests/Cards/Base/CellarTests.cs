using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class CellarTests : CardTestsBase
{
	private readonly Card cellar = Cellar.Get();
	private readonly Card copper = Copper.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(cellar));
	}

	[TestMethod]
	public void DrawNoCards()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 action
		Assert.AreEqual(1, player.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player does not discard any card
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player does not draw any card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DrawOneCard()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 action
		Assert.AreEqual(1, player.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards the chosen card
		player.Verify(p => p.Discard(copper), Times.Once);

		// player draws one card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DrawFourCards()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, cellar, copper });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 action
		Assert.AreEqual(1, player.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards the chosen cards
		player.Verify(p => p.Discard(copper), Times.Exactly(3));
		player.Verify(p => p.Discard(cellar), Times.Once);

		// player draws four cards
		player.Verify(p => p.Draw(4), Times.Once);
		#endregion
	}
}