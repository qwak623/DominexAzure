using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerVillageTests : CardWithPlayerTestsBase
{
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(village);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { village };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.PlayActionCardInternal(village);
		#endregion

		#region assert
		// (-1 Action, +2 Actions), +0 Coins, +0 Buys
		Assert.AreEqual(2, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// village was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, village };
		player.PlayerState.DrawPile = new List<Card> { copper, copper };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == village))).Returns(village);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+2 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(4, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// (+1 Card) * 2
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// village and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { village, throneRoom }, player.PlayerState.CardsPlayed);

		// two villages and throne room were added to actions played
		CollectionAssert.AreEquivalent(new List<Card> { village, village, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}