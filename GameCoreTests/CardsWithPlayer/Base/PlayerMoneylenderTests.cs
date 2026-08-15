using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMoneylenderTests : CardWithPlayerTestsBase
{
	private readonly Card moneylender = Moneylender.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(moneylender);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void TrashCopper()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([moneylender, copper]);
		var moneylenderToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Moneylender);
		user.Setup(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(moneylenderToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 3, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([moneylender], player.PlayerState.CardsPlayed);
		AssertPile([moneylender], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);

		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToTrashCopper()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([moneylender, copper]);
		var moneylenderToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Moneylender);
		user.Setup(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(moneylenderToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([moneylender], player.PlayerState.CardsPlayed);
		AssertPile([moneylender], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyCopper()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([moneylender]);
		var moneylenderToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(moneylenderToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([moneylender], player.PlayerState.CardsPlayed);
		AssertPile([moneylender], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// no copper to trash, so the user is never asked
		user.Verify(u => u.MoneylenderTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	[DataRow(2, true, true, 6, 2, 2)]
	[DataRow(2, true, false, 3, 2, 1)]
	[DataRow(2, false, true, 3, 2, 1)]
	[DataRow(2, false, false, 0, 2, 0)]
	[DataRow(1, true, true, 3, 1, 1)]
	[DataRow(1, false, true, 3, 2, 1)]
	[DataRow(1, false, false, 0, 2, 0)]
	[DataRow(0, true, true, 0, 0, 0)]
	public void ThroneRoomPlay(int coppersInHand, bool trashFirstTime, bool trashSecondTime, int coins, int askedToTrash, int trashCount)
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, moneylender, .. Enumerable.Repeat(copper, coppersInHand)]);
		var moneylenderToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Moneylender);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == moneylenderToPlay))).Returns(moneylenderToPlay);
		user.SetupSequence(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom))
			.Returns(trashFirstTime).Returns(trashSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, coins, 0, player);
		AssertPile(Enumerable.Repeat(copper, coppersInHand - trashCount).ToList(), player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([moneylender, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([moneylender, moneylender, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile(Enumerable.Repeat(copper, trashCount).ToList(), player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the second resolution only asks if a copper is still left to offer - e.g. the (1, true,
		// true, ...) case trashes the only copper on the first resolution, so Hand.Any(copper) is
		// false by the second and MoneylenderTrash is never called for it
		user.Verify(u => u.MoneylenderTrash(moneylender, player.PlayerState, player.Game.Kingdom), Times.Exactly(askedToTrash));
		#endregion
	}
}
