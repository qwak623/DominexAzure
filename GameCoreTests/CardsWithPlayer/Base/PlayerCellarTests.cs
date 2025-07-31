using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerCellarTests : CardWithPlayerTestsBase
{
	private readonly Card cellar = Cellar.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(cellar);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DrawNoCards()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { silver, silver, cellar, copper, silver };

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { });
		#endregion

		#region act
		player.PlayActionCardInternal(cellar);
		#endregion

		#region assert
		// -1 action, +1 action
		Assert.AreEqual(1, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);

		// nothing was discarded
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// hand stayed the same except for the played cellar
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver, copper, silver }, player.PlayerState.Hand);

		// cellar was added to played cards
		CollectionAssert.AreEqual(new List<Card> { cellar }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DrawOneCard()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { gold };
		player.PlayerState.Hand = new List<Card> { silver, silver, cellar, copper, silver };

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		player.PlayActionCardInternal(cellar);
		#endregion

		#region assert
		// -1 action, +1 action
		Assert.AreEqual(1, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);

		// the copper was discarded
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DiscardPile);

		// the player draws one card - a gold
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver, silver, gold }, player.PlayerState.Hand);

		// the gold was removed from the draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// cellar was added to played cards
		CollectionAssert.AreEqual(new List<Card> { cellar }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DrawFourCards()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { gold, gold, gold, gold };
		player.PlayerState.Hand = new List<Card> { silver, copper, cellar, copper, cellar };

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, cellar, silver });
		#endregion

		#region act
		player.PlayActionCardInternal(cellar);
		#endregion

		#region assert
		// -1 action, +1 action
		Assert.AreEqual(1, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);

		// the cards are discarded
		CollectionAssert.AreEquivalent(new List<Card> { copper, cellar, copper, silver }, player.PlayerState.DiscardPile);

		// the player draws four cards
		CollectionAssert.AreEquivalent(new List<Card> { gold, gold, gold, gold }, player.PlayerState.Hand);

		// the cards were removed from the draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// cellar was added to played cards
		CollectionAssert.AreEqual(new List<Card> { cellar }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DrawTheSameCardBack()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { silver, copper, cellar, copper, cellar };

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		player.PlayActionCardInternal(cellar);
		#endregion

		#region assert
		// -1 action, +1 action
		Assert.AreEqual(1, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);

		// the card was discarded, but it was mixed to the draw pile afterwards
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// the player draws the same copper back
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, copper, cellar }, player.PlayerState.Hand);

		// nothing stayed on the draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// cellar was added to played cards
		CollectionAssert.AreEqual(new List<Card> { cellar }, player.PlayerState.PlayedCards);
		#endregion
	}
}