using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
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

		game.Setup(g => g.Players).Returns([player2, player, player3, player4]);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([councilRoom, copper]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, silver, councilRoom]);
		player2.PlayerState.DrawPile = CreatePile([copper]);
		player3.PlayerState.DrawPile = CreatePile([silver]);
		var councilRoomToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.CouncilRoom);

		#endregion

		#region act
		player.PlayActionCardInternal(councilRoomToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 1, player);
		AssertPile([copper, copper, silver, silver, councilRoom], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([councilRoom], player.PlayerState.CardsPlayed);
		AssertPile([councilRoom], player.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player2);
		AssertPile([copper], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);
		AssertPile([], player2.PlayerState.CardsPlayed);
		AssertPile([], player2.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player3);
		AssertPile([silver], player3.PlayerState.Hand);
		AssertPile([], player3.PlayerState.DrawPile);
		AssertPile([], player3.PlayerState.DiscardPile);
		AssertPile([], player3.PlayerState.CardsPlayed);
		AssertPile([], player3.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player4);
		AssertPile([], player4.PlayerState.Hand);
		AssertPile([], player4.PlayerState.DrawPile);
		AssertPile([], player4.PlayerState.DiscardPile);
		AssertPile([], player4.PlayerState.CardsPlayed);
		AssertPile([], player4.PlayerState.ActionsPlayed);

		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, councilRoom]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, silver, councilRoom, copper, silver, councilRoom, copper]);
		player2.PlayerState.DrawPile = CreatePile([copper, silver]);
		player3.PlayerState.DrawPile = CreatePile([silver]);
		player4.PlayerState.DrawPile = CreatePile([]);
		var councilRoomToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.CouncilRoom);
		var throneRoomToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == councilRoomToPlay))).Returns(councilRoomToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoomToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 2, player);
		AssertPile([copper, silver, silver, councilRoom, copper, silver, councilRoom, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, councilRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, councilRoom, councilRoom], player.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player2);
		AssertPile([copper, silver], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);
		AssertPile([], player2.PlayerState.CardsPlayed);
		AssertPile([], player2.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player3);
		AssertPile([silver], player3.PlayerState.Hand);
		AssertPile([], player3.PlayerState.DrawPile);
		AssertPile([], player3.PlayerState.DiscardPile);
		AssertPile([], player3.PlayerState.CardsPlayed);
		AssertPile([], player3.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player4);
		AssertPile([], player4.PlayerState.Hand);
		AssertPile([], player4.PlayerState.DrawPile);
		AssertPile([], player4.PlayerState.DiscardPile);
		AssertPile([], player4.PlayerState.CardsPlayed);
		AssertPile([], player4.PlayerState.ActionsPlayed);

		AssertPile([], player.Game.Trash);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}
}