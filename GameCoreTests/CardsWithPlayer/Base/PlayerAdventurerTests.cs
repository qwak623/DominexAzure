using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerAdventurerTests
{
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card province = Province.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		var kingdom = new Kingdom(new() { adventurer }, 2);

		game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);
		game.Setup(g => g.Trash).Returns(new List<Card> { });

		user = new Mock<IUser>();

		player = new Player(game.Object, user.Object);
		player.PlayerState.Actions = 1;
		player.PlayerState.Buys = 0;
		player.PlayerState.Coins = 0;
		player.PlayerState.PlayedCards = new List<Card> { };
	}

	[TestMethod]
	public void DrawTwoTreasures()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { copper, silver };
		player.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// the draw pile is empty
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// the discard pile is empty
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, silver }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEqual(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void SkipNonTreasures()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { province, gold, silver, adventurer, adventurer, province };
		player.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// a province stayed in the draw pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, player.PlayerState.DrawPile);

		// the non-treasure cards are on the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province, adventurer, adventurer }, player.PlayerState.DiscardPile);

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, gold }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEqual(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void OneTreasureToDraw()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { gold, province };
		player.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// the draw pile is empty
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// the non-treasure card is on the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, player.PlayerState.DiscardPile);

		// player has the treasure in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, gold }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEqual(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void NoTreasuresToDraw()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { };
		player.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// the draw pile is empty
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// the discard pile is empty
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// nothing was added to the player's hand
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEqual(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}
}