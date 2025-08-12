using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerAdventurerTests : CardWithPlayerTestsBase
{
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
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
		game = MockGame(adventurer);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DrawTwoTreasures()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { copper, silver };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, silver }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void SkipNonTreasures()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { province, gold, silver, adventurer, adventurer, province };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { province }, player.PlayerState.DrawPile);

		// the non-treasure cards are on the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province, adventurer, adventurer }, player.PlayerState.DiscardPile);

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, gold }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void OneTreasureToDraw()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		player.PlayerState.DrawPile = new List<Card> { gold, province };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// the non-treasure card is on the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, player.PlayerState.DiscardPile);

		// player has the treasure in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, gold }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void NoTreasuresToDraw()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(adventurer);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// nothing was added to the player's hand
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// adventurer was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomFourTreasures()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, throneRoom };
		player.PlayerState.DrawPile = new List<Card> { silver, province, copper, province, province, gold, province, silver, adventurer, province, copper };

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(adventurer)))).Returns(adventurer);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { province, silver }, player.PlayerState.DrawPile);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player gained the treasures to the hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, gold, copper }, player.PlayerState.Hand);

		// non-treasure cards were discarded
		CollectionAssert.AreEquivalent(new List<Card> { province, province, province, adventurer, province }, player.PlayerState.DiscardPile);

		// throne room and adventurer were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	public void ThroneRoomNotEnoughTreasures(int treasureCount)
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { adventurer, throneRoom };
		player.PlayerState.DrawPile = new List<Card> { province, province, province, province, adventurer, province };
		player.PlayerState.DrawPile.AddRange(Enumerable.Repeat(gold, treasureCount));

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(adventurer)))).Returns(adventurer);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player gained the treasures to the hand
		CollectionAssert.AreEquivalent(Enumerable.Repeat(gold, treasureCount).ToList(), player.PlayerState.Hand);

		// non-treasure cards were discarded
		CollectionAssert.AreEquivalent(new List<Card> { province, province, province, province, adventurer, province }, player.PlayerState.DiscardPile);

		// throne room and adventurer were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom, adventurer }, player.PlayerState.PlayedCards);
		#endregion
	}
}