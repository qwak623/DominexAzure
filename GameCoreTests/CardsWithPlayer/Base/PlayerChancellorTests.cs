using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerChancellorTests : CardWithPlayerTestsBase
{
	private readonly Card chancellor = Chancellor.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(chancellor);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(true);

		player.PlayerState.Hand = CreatePile([chancellor, copper]);
		player.PlayerState.DiscardPile = CreatePile([copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var chancellorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chancellor);
		#endregion

		#region act
		player.PlayActionCardInternal(chancellorToPlay);
		#endregion

		#region assert
		// player discards his draw pile
		AssertNumbers(0, 2, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper, silver, silver], player.PlayerState.DiscardPile);
		AssertPile([chancellor], player.PlayerState.CardsPlayed);
		AssertPile([chancellor], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontDiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(false);

		player.PlayerState.Hand = CreatePile([chancellor, copper]);
		player.PlayerState.DiscardPile = CreatePile([copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var chancellorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chancellor);
		#endregion

		#region act
		player.PlayActionCardInternal(chancellorToPlay);
		#endregion

		#region assert
		// player doesn't discard his draw pile
		AssertNumbers(0, 2, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([silver, silver], player.PlayerState.DrawPile);
		AssertPile([copper], player.PlayerState.DiscardPile);
		AssertPile([chancellor], player.PlayerState.CardsPlayed);
		AssertPile([chancellor], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	[DataRow(true, true, DisplayName = "DiscardsBothTimes")]
	[DataRow(true, false, DisplayName = "OnlyDiscardsForTheFirstTime")]
	[DataRow(false, true, DisplayName = "OnlyDiscardsForTheSecondTime")]
	public void ThroneRoomDiscardDrawPile(bool discardFirstTime, bool discardSecondTime)
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, chancellor, copper]);
		player.PlayerState.DiscardPile = CreatePile([copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var chancellorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chancellor);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == chancellorToPlay))).Returns(chancellorToPlay);
		user.SetupSequence(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// player discards his draw pile
		AssertNumbers(0, 4, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper, silver, silver], player.PlayerState.DiscardPile);
		AssertPile([chancellor, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([chancellor, chancellor, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDontDiscardDrawPile()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, chancellor, copper]);
		player.PlayerState.DiscardPile = CreatePile([copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var chancellorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chancellor);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == chancellorToPlay))).Returns(chancellorToPlay);
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// player doesn't discard his draw pile
		AssertNumbers(0, 4, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([silver, silver], player.PlayerState.DrawPile);
		AssertPile([copper], player.PlayerState.DiscardPile);
		AssertPile([chancellor, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([chancellor, chancellor, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));
		#endregion
	}
}
