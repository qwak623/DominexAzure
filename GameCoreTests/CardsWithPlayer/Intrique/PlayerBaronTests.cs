using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerBaronTests : CardWithPlayerTestsBase
{
	private readonly Card baron = Baron.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(baron);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardEstate()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { baron, estate };
		user.Setup(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(baron);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +4 Coins, +1 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// the estate should be in the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { estate }, player.PlayerState.DiscardPile);

		// user is asked to choose whether to discard an estate
		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Once);

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// baron was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { baron }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToDiscardEstate()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { baron, estate };
		user.Setup(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(baron);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +0 Coins, +1 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked to choose whether to trash a copper
		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the estate should stay in the players hand
		CollectionAssert.AreEquivalent(new List<Card> { estate }, player.PlayerState.Hand);

		// baron was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { baron }, player.PlayerState.PlayedCards);

		// player gained an estate
		CollectionAssert.AreEquivalent(new List<Card> { estate }, player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyEstate()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { baron };
		#endregion

		#region act
		player.PlayActionCardInternal(baron);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +0 Coins, +1 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user isn't asked to choose whether to discard an estate - he doesn't have any
		user.Verify(u => u.BaronDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// baron was added to the played cards
		CollectionAssert.AreEquivalent(new List<Card> { baron }, player.PlayerState.PlayedCards);

		// player gained an estate
		CollectionAssert.AreEquivalent(new List<Card> { estate }, player.PlayerState.DiscardPile);
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
		player.PlayerState.Hand = [throneRoom, baron, .. Enumerable.Repeat(estate, estatesInHand)];

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == baron))).Returns(baron);
		user.SetupSequence(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +given amount of coins
		Assert.AreEqual(coins, player.PlayerState.Coins);

		// -1 Action, (+0 Actions, +1 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to discard an estate the given amount of times
		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Exactly(askedToDiscard));

		// player discards the given amount of estates
		CollectionAssert.AreEquivalent(Enumerable.Repeat(estate, estatesInHand - discardCount).ToList(), player.PlayerState.Hand);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// player gained or discarded two estates
		CollectionAssert.AreEquivalent(Enumerable.Repeat(estate, 2).ToList(), player.PlayerState.DiscardPile);

		// baron and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { baron, throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}
}