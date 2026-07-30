using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerLibraryTests : CardWithPlayerTestsBase
{
	private readonly Card library = Library.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card province = Province.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { library, adventurer });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void AlreadyHas7Cards()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { copper, copper, library, adventurer, silver, silver, gold, province };
		#endregion

		#region act
		player.PlayActionCardInternal(library);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user does not need to decide whether to skip action card
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// the hand hasn't changed 
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, adventurer, silver, silver, gold, province }, player.PlayerState.Hand);

		// nothing was discarded
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// library was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper, silver };
		player.PlayerState.DrawPile = new List<Card> { library, adventurer, copper };
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(library);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user needs to decide whether to skip two action cards
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// the hand should now include copper and adventurer
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, copper, adventurer }, player.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.DiscardPile);

		// library was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDrawToSevenCards()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper, silver, library, throneRoom };
		player.PlayerState.DrawPile = new List<Card> { copper, silver, library, adventurer, copper };
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library)).Returns(true);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(library)))).Returns(library);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user needs to decide whether to skip two action cards
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), silver), Times.Never);

		// the hand should now have 7 cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, copper, adventurer, silver, copper }, player.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.DiscardPile);

		// throne room and library were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, library }, player.PlayerState.CardsPlayed);

		// throne room and two libraries were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, library, library }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNotEnoughCards()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper, silver, library, throneRoom };
		player.PlayerState.DrawPile = new List<Card> { copper, throneRoom, library, adventurer };
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library)).Returns(true);
		user.SetupSequence(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, throneRoom)).Returns(true).Returns(false);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(library)))).Returns(library);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user needs to decide whether to skip three action cards
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library), Times.Exactly(2));
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, throneRoom), Times.Exactly(2));

		// user does not need to decide whether to skip non-action cards
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// player gained the cards to the hand
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, adventurer, throneRoom, copper }, player.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.PlayerState.DiscardPile);

		// throne room and library were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, library }, player.PlayerState.CardsPlayed);

		// throne room and two libraries were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, library, library }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}