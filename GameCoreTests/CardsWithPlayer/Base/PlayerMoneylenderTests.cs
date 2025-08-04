using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMoneylenderTests : CardWithPlayerTestsBase
{
	private readonly Card moneylender = Moneylender.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(moneylender);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void TrashCopper()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { moneylender, copper };
		user.Setup(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(moneylender);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +3 Coins
		Assert.AreEqual(3, player.PlayerState.Coins);

		// Buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose whether to trash a copper
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes the copper
		CollectionAssert.AreEqual(new List<Card> { copper }, player.Game.Trash.ToList());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// moneylender was added to played cards
		CollectionAssert.AreEqual(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToTrashCopper()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { moneylender, copper };
		user.Setup(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(moneylender);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose whether to trash a copper
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the copper should stay in the players hand
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// moneylender was added to played cards
		CollectionAssert.AreEqual(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyCopper()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { moneylender };
		#endregion

		#region act
		player.PlayActionCardInternal(moneylender);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user isnt asked to choose whether to trash a copper - he doesnt have any
		user.Verify(u => u.MoneylenderTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player doesnt trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// moneylender was added to played cards
		CollectionAssert.AreEqual(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
		#endregion
	}
}