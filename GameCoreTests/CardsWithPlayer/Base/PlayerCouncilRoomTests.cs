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
	public void OtherPlayersDrawCard()
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
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +1 Buy
		Assert.AreEqual(1, player.PlayerState.Buys);

		// coins shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);

		// player draws 4 cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, silver, silver, councilRoom }, player.PlayerState.Hand);

		// the four cards were removed from the draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// council room was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { councilRoom }, player.PlayerState.PlayedCards);

		// players 2 and 3 draw one card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player2.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player3.PlayerState.Hand);

		// player 4 does not have any card to draw
		Assert.IsFalse(player4.PlayerState.Hand.Any());

		// all the other players' draw piles are empty
		Assert.IsFalse(player2.PlayerState.DrawPile.Any());
		Assert.IsFalse(player3.PlayerState.DrawPile.Any());
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		#endregion
	}
}