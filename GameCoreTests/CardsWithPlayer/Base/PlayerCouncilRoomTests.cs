using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerCouncilRoomTests : CardWithPlayerTestsBase
{
	private readonly Card councilRoom = CouncilRoom.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;
	private Player player2;
	private Player player3;
	private Player player4;

	private Mock<IUser> user;
	private Mock<IUser> user2;
	private Mock<IUser> user3;
	private Mock<IUser> user4;

	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(councilRoom);

		user = new Mock<IUser>();
		user2 = new Mock<IUser>();
		user3 = new Mock<IUser>();
		user4 = new Mock<IUser>();

		player = CreatePlayer(game.Object, user.Object);
		player2 = CreatePlayer(game.Object, user2.Object);
		player3 = CreatePlayer(game.Object, user3.Object);
		player4 = CreatePlayer(game.Object, user4.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { player2, player, player3, player4 });
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { councilRoom, copper };
		player.PlayerState.DrawPile = new List<Card> { copper, silver, silver, councilRoom };
		player2.PlayerState.DrawPile = new List<Card> { copper };
		player3.PlayerState.DrawPile = new List<Card> { silver };
		#endregion

		#region act
		player.PlayActionCardInternal(councilRoom);
		#endregion

		#region assert
		// (-1 Action, +0 Actions), +0 Coins, +1 Buy 
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +4 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, silver, silver, councilRoom }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// council room was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { councilRoom }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { councilRoom }, player.PlayerState.ActionsPlayed);

		// players 2 and 3 draw one card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player2.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player3.PlayerState.Hand);

		// player 4 does not have any card to draw
		Assert.IsFalse(player4.PlayerState.Hand.Any());

		// all the other players' draw piles and discard piles are empty
		Assert.IsFalse(player2.PlayerState.DrawPile.Any());
		Assert.IsFalse(player3.PlayerState.DrawPile.Any());
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		Assert.IsFalse(player2.PlayerState.DiscardPile.Any());
		Assert.IsFalse(player3.PlayerState.DiscardPile.Any());
		Assert.IsFalse(player4.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, councilRoom };
		player.PlayerState.DrawPile = new List<Card> { copper, silver, silver, councilRoom, copper, silver, councilRoom, copper };
		player2.PlayerState.DrawPile = new List<Card> { copper, silver };
		player3.PlayerState.DrawPile = new List<Card> { silver };

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == councilRoom))).Returns(councilRoom);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +0 Coins, +1 Buy) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(2, player.PlayerState.Buys);

		// (+4 Cards) * 2
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver, councilRoom, copper, silver, councilRoom, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// council room and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { councilRoom, throneRoom }, player.PlayerState.CardsPlayed);

		// two council rooms and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { councilRoom, councilRoom, throneRoom }, player.PlayerState.ActionsPlayed);

		// player 2 draws 2 cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, player2.PlayerState.Hand);

		//  player 3 draws one card
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player3.PlayerState.Hand);

		// player 4 does not have any card to draw
		Assert.IsFalse(player4.PlayerState.Hand.Any());

		// all the other players' draw piles and discard piles are empty
		Assert.IsFalse(player2.PlayerState.DrawPile.Any());
		Assert.IsFalse(player3.PlayerState.DrawPile.Any());
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		Assert.IsFalse(player2.PlayerState.DiscardPile.Any());
		Assert.IsFalse(player3.PlayerState.DiscardPile.Any());
		Assert.IsFalse(player4.PlayerState.DiscardPile.Any());
		#endregion
	}
}