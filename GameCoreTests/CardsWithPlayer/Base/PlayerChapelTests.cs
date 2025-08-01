using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerChapelTests : CardWithPlayerTestsBase
{
	private readonly Card chapel = Chapel.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(chapel);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
		player.PlayerState.Hand = new List<Card> { copper, silver, silver, copper, chapel };
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom)).Returns(new List<Card> { });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player does not draw or trash any card
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.Game.Trash.Any());

		// chapel was added to played cards
		CollectionAssert.AreEqual(new List<Card> { chapel }, player.PlayerState.PlayedCards);
		#endregion
	}
	[TestMethod]
	public void TrashOneCard()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom)).Returns(new List<Card> { copper });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes a copper
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.Game.Trash.ToList());

		// chapel was added to played cards
		CollectionAssert.AreEqual(new List<Card> { chapel }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void TrashFourCards()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes four cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver, copper }, player.Game.Trash.ToList());

		// chapel was added to played cards
		CollectionAssert.AreEqual(new List<Card> { chapel }, player.PlayerState.PlayedCards);
		#endregion
	}
}