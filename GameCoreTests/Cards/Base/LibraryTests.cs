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
	public void AlreadyHas7Cards()
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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player does not show any cards
		player.Verify(p => p.Show(It.IsAny<int>()), Times.Never);

		// user does not need to decide whether to skip action card
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// the hand hasn't changed 
		CollectionAssert.AreEqual(hand, player.Object.PlayerState.Hand);

		// nothing was discarded
		Assert.IsFalse(player.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Play()
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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows 3 cards (in the last call he does not have any)
		player.Verify(p => p.Show(1), Times.Exactly(4));

		// user needs to decide whether to skip two action cards
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, adventurer), Times.Once);
		player.Verify(p => p.User.LibrarySkip(library, player.Object.PlayerState, player.Object.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		player.Verify(p => p.User.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// the hand should now include copper and adventurer
		CollectionAssert.AreEqual(new List<Card> { adventurer, copper, silver, copper, adventurer }, player.Object.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEqual(new List<Card> { library }, player.Object.PlayerState.DiscardPile);
		#endregion
	}
}