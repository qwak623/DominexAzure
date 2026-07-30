using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerConspiratorTests : CardWithPlayerTestsBase
{
	private readonly Card conspirator = Conspirator.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(conspirator);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PlayAsFirstCard()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { conspirator };
		player.PlayerState.DrawPile = [copper];
		#endregion

		#region act
		player.PlayActionCardInternal(conspirator);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +2 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DrawPile);

		// the discard pile should be empty
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// conspirator was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { conspirator }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { conspirator }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void PlayAsSecondCard()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { conspirator };
		player.PlayerState.ActionsPlayed.Add(conspirator);
		player.PlayerState.CardsPlayed.Add(conspirator);
		player.PlayerState.DrawPile = [copper];
		#endregion

		#region act
		player.PlayActionCardInternal(conspirator);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +2 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DrawPile);

		// the discard pile should be empty
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// the hand should be empty
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// conspirator was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, conspirator }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, conspirator }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void PlayAsThirdCard()
	{
		#region arrange
		player.PlayerState.Hand = [conspirator];
		player.PlayerState.ActionsPlayed = [conspirator, conspirator];
		player.PlayerState.CardsPlayed = [conspirator, conspirator];
		player.PlayerState.DrawPile = [copper];
		#endregion

		#region act
		player.PlayActionCardInternal(conspirator);
		#endregion

		#region assert
		// -1 Action, (+1 Actions +2 Coins, +0 Buys)
		Assert.AreEqual(1, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +1 Card
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// baron was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, conspirator, conspirator }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, conspirator, conspirator }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsFirst()
	{
		#region arrange
		player.PlayerState.Hand = [throneRoom, conspirator];
		player.PlayerState.DrawPile = [copper, copper];

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == conspirator))).Returns(conspirator);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +1 Action
		Assert.AreEqual(1, player.PlayerState.Actions);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// conspirator and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, conspirator }, player.PlayerState.CardsPlayed);

		// two conspirators and throne room were added to actions played
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, conspirator, conspirator }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsSecond()
	{
		#region arrange
		player.PlayerState.ActionsPlayed.Add(conspirator);
		player.PlayerState.CardsPlayed.Add(conspirator);
		player.PlayerState.Hand = [throneRoom, conspirator];
		player.PlayerState.DrawPile = [copper, copper];

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == conspirator))).Returns(conspirator);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +2 Action
		Assert.AreEqual(2, player.PlayerState.Actions);

		// +2 Card
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, player.PlayerState.Hand);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// conspirator and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, throneRoom, conspirator }, player.PlayerState.CardsPlayed);

		// two conspirators and throne room were added to actions played
		CollectionAssert.AreEquivalent(new List<Card> { conspirator, throneRoom, conspirator, conspirator }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}