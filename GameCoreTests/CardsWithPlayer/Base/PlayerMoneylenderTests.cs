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
	private readonly Card throneRoom = ThroneRoom.Get();

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
		// (-1 Action, +0 Actions), +3 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(3, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked to choose whether to trash a copper
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes the copper
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.Game.Trash.ToList());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// moneylender was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
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
		// (-1 Action, +0 Actions), +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked to choose whether to trash a copper
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the copper should stay in the players hand
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// moneylender was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
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
		// (-1 Action, +0 Actions), +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user isnt asked to choose whether to trash a copper - he doesnt have any
		user.Verify(u => u.MoneylenderTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player doesnt trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// moneylender was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { moneylender }, player.PlayerState.PlayedCards);
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
		player.PlayerState.Hand = new List<Card> { throneRoom, moneylender };
		player.PlayerState.Hand.AddRange(Enumerable.Repeat(copper, coppersInHand));

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == moneylender))).Returns(moneylender);
		user.SetupSequence(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom))
			.Returns(trashFirstTime).Returns(trashSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +given amount of coins
		Assert.AreEqual(coins, player.PlayerState.Coins);

		// -1 Action, (+0 Actions, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to trash a copper given amout of times
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Exactly(askedToTrash));

		// player trashes the given amount of coppers
		CollectionAssert.AreEquivalent(Enumerable.Repeat(copper, coppersInHand - trashCount).ToList(), player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(Enumerable.Repeat(copper, trashCount).ToList(), player.Game.Trash.ToList());

		// moneylender and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { moneylender, throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}
}