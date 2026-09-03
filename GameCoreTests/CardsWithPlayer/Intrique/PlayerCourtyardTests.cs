using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerCoutyardTests : CardWithPlayerTestsBase
{
	private readonly Card courtyard = Courtyard.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card estate = Estate.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(courtyard);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void ReturnDrawnCard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([courtyard]);
		player.PlayerState.DrawPile = CreatePile([copper, throneRoom, courtyard]);
		var copperToReturn = player.PlayerState.DrawPile.First(c => c.Card.Name == CardName.Copper);

		user.Setup(u => u.CourtyardPutOnTop(It.IsAny<Card>(), player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copperToReturn);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([throneRoom, courtyard], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([courtyard], player.PlayerState.CardsPlayed);
		AssertPile([courtyard], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ReturnCardFromHand()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([courtyard, copper]);
		player.PlayerState.DrawPile = CreatePile([throneRoom, throneRoom, courtyard]);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);

		user.Setup(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([throneRoom, courtyard, throneRoom], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([courtyard], player.PlayerState.CardsPlayed);
		AssertPile([courtyard], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void CantReturnCard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([courtyard]);
		player.PlayerState.DrawPile = CreatePile([]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([courtyard], player.PlayerState.CardsPlayed);
		AssertPile([courtyard], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, courtyard, estate]);
		player.PlayerState.DrawPile = CreatePile([.. Enumerable.Repeat(copper, 6)]);
		var courtyardToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Courtyard);
		var copperToDraw = player.PlayerState.DrawPile.First(c => c.Card.Name == CardName.Copper);
		var estateInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Estate);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == courtyardToPlay))).Returns(courtyardToPlay);
		user.Setup(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(estateInHand);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([.. Enumerable.Repeat(copper, 5)], player.PlayerState.Hand);
		AssertPile([copper, estate], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, courtyard], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, courtyard, courtyard], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		user.Verify(u => u.CourtyardPutOnTop(courtyard, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Exactly(2));
		#endregion
	}
}
