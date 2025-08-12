using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass()]
public class LibraryTests : CardTestsBase
{
	private readonly Card library = Library.Get();
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card province = Province.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(library));
	}

	[TestMethod]
	public void AlreadyHasSevenCards()
	{
		#region arrange
		var hand = new List<Card> { copper, copper, adventurer, silver, silver, gold, province };
		player.Object.PlayerState.DiscardPile = new List<Card> { };
		player.Object.PlayerState.Hand = hand.ToList();
		#endregion

		#region act
		library.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player does not show any cards
		player.Verify(p => p.Show(It.IsAny<int>()), Times.Never);

		// user does not need to decide whether to skip action card
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// the hand hasn't changed 
		CollectionAssert.AreEquivalent(hand, player.Object.PlayerState.Hand);

		// nothing was discarded
		Assert.IsFalse(player.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void DoesntHaveEnoughCards()
	{
		#region arrange
		var hand = new List<Card> { adventurer, copper, silver };
		player.Object.PlayerState.DiscardPile = new List<Card> { };
		player.Object.PlayerState.Hand = hand.ToList();
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { library })
			.Returns(new List<Card> { });
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer)).Returns(false);
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library)).Returns(true);
		#endregion

		#region act
		library.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows 3 cards (in the last call he does not have any)
		player.Verify(p => p.Show(1), Times.Exactly(4));

		// user needs to decide whether to skip two action cards
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer), Times.Once);
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// the hand should now include copper and adventurer
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, copper, adventurer }, player.Object.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.Object.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void DrawToSevenCards()
	{
		#region arrange
		var hand = new List<Card> { adventurer, copper, silver };

		player.Object.PlayerState.DiscardPile = new List<Card> { };
		player.Object.PlayerState.Hand = hand.ToList();
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { library })
			.Returns(new List<Card> { silver })
			.Returns(new List<Card> { copper });
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer)).Returns(false);
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library)).Returns(true);
		#endregion

		#region act
		library.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows 5 cards
		player.Verify(p => p.Show(1), Times.Exactly(5));

		// user needs to decide whether to skip two action cards
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer), Times.Once);
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), silver), Times.Never);

		// the hand should now have 7 cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, copper, adventurer, silver, copper }, player.Object.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.Object.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDrawToSevenCards()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { adventurer, copper, silver, library };

		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { library })
			.Returns(new List<Card> { silver })
			.Returns(new List<Card> { copper });
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer)).Returns(false);
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library)).Returns(true);
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(library)))).Returns(library);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player shows 5 cards
		player.Verify(p => p.Show(1), Times.Exactly(5));

		// user needs to decide whether to skip two action cards
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer), Times.Once);
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), silver), Times.Never);

		// the hand should now have 7 cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, copper, adventurer, silver, copper }, player.Object.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEquivalent(new List<Card> { library }, player.Object.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNotEnoughCards()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { adventurer, copper, silver, library };

		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { library })
			.Returns(new List<Card> { throneRoom })
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { })
			.Returns(new List<Card> { library })
			.Returns(new List<Card> { throneRoom })
			.Returns(new List<Card> { });
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer)).Returns(false);
		player.Setup(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library)).Returns(true);
		player.SetupSequence(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, throneRoom)).Returns(true).Returns(false);
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(library)))).Returns(library);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player shows 8 cards
		player.Verify(p => p.Show(1), Times.Exactly(8));

		// user needs to decide whether to skip three action cards
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer), Times.Once);
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library), Times.Exactly(2));
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, throneRoom), Times.Exactly(2));

		// user does not need to decide whether to skip non-action cards
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// player gained the cards to the hand
		CollectionAssert.AreEquivalent(new List<Card> { adventurer, copper, silver, adventurer, throneRoom, copper }, player.Object.PlayerState.Hand);

		// library, throne room and library were discarded (only library stayed there)
		CollectionAssert.AreEquivalent(new List<Card> { library, throneRoom, library }, player.Object.PlayerState.DiscardPile);
		#endregion
	}
}