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

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(library);
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
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user does not need to decide whether to skip action card
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// the hand hasn't changed 
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, adventurer, silver, silver, gold, province }, player.PlayerState.Hand);

		// nothing was discarded
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// library was added to played cards
		CollectionAssert.AreEqual(new List<Card> { library }, player.PlayerState.PlayedCards);
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
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user needs to decide whether to skip two action cards
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurer), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, library), Times.Once);

		// user does not need to decide whether to skip non-action cards
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), copper), Times.Never);

		// the hand should now include copper and adventurer
		CollectionAssert.AreEqual(new List<Card> { adventurer, copper, silver, copper, adventurer }, player.PlayerState.Hand);

		// library was discarded
		CollectionAssert.AreEqual(new List<Card> { library }, player.PlayerState.DiscardPile);

		// library was added to played cards
		CollectionAssert.AreEqual(new List<Card> { library }, player.PlayerState.PlayedCards);
		#endregion
	}
}