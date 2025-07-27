using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class ChapelTests : CardTestsBase
{
	private readonly Card chapel = Chapel.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(chapel));
		player.Object.PlayerState.Hand = new List<Card> { copper, silver, silver, copper, chapel };
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player does not trash anything
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);
		#endregion
	}
	[TestMethod]
	public void TrashOneCard()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the card
		player.Verify(p => p.Trash(copper), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TrashFourCards()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the cards
		player.Verify(p => p.Trash(copper), Times.Exactly(2));
		player.Verify(p => p.Trash(silver), Times.Exactly(2));
		#endregion
	}
}