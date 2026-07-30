using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMarketTests : CardWithPlayerTestsBase
{
	private readonly Card market = Market.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(market);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { market };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.PlayActionCardInternal(market);
		#endregion

		#region assert
		// (-1 Action, +1 Actions), +1 Coin, +1 Buy
		Assert.AreEqual(1, player.PlayerState.Actions);
		Assert.AreEqual(1, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// market was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { market }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { market }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, market };
		player.PlayerState.DrawPile = new List<Card> { copper, copper };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == market))).Returns(market);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+1 Action, +1 Coin, +1 Buy) * 2
		Assert.AreEqual(2, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(2, player.PlayerState.Buys);

		// (+1 Card) * 2
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// market and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { market, throneRoom }, player.PlayerState.CardsPlayed);

		// two markets and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { market, market, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}