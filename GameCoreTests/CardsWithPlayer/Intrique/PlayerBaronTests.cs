using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerBaronTests : CardWithPlayerTestsBase
{
	private readonly Card baron = Baron.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(baron);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardEstate()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([baron, estate]);
		var baronToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Baron);
		user.Setup(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(baronToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 4, 1, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([estate], player.PlayerState.DiscardPile);
		AssertPile([baron], player.PlayerState.CardsPlayed);
		AssertPile([baron], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesntWantToDiscardEstate()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([baron, estate]);
		var baronToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Baron);
		user.Setup(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(baronToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 1, player);
		AssertPile([estate], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([estate], player.PlayerState.DiscardPile);
		AssertPile([baron], player.PlayerState.CardsPlayed);
		AssertPile([baron], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntHaveAnyEstate()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([baron]);
		var baronToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(baronToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 1, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([estate], player.PlayerState.DiscardPile);
		AssertPile([baron], player.PlayerState.CardsPlayed);
		AssertPile([baron], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// no estate to discard, so the user is never asked - baron just gains one instead
		user.Verify(u => u.BaronDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	[DataRow(2, true, true, 8, 2, 2)]
	[DataRow(2, true, false, 4, 2, 1)]
	[DataRow(2, false, true, 4, 2, 1)]
	[DataRow(2, false, false, 0, 2, 0)]
	[DataRow(1, true, true, 4, 1, 1)]
	[DataRow(1, false, true, 4, 2, 1)]
	[DataRow(1, false, false, 0, 2, 0)]
	[DataRow(0, true, true, 0, 0, 0)]
	public void ThroneRoomPlay(int estatesInHand, bool discardFirstTime, bool discardSecondTime, int coins, int askedToDiscard, int discardCount)
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, baron, .. Enumerable.Repeat(estate, estatesInHand)]);
		var baronToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Baron);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == baronToPlay))).Returns(baronToPlay);
		user.SetupSequence(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, coins, 2, player);
		AssertPile(Enumerable.Repeat(estate, estatesInHand - discardCount).ToList(), player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);

		// every resolution either discards an estate for coins or gains one instead - the discard
		// pile always ends up with exactly two, whichever mix of the two happened
		AssertPile(Enumerable.Repeat(estate, 2).ToList(), player.PlayerState.DiscardPile);
		AssertPile([baron, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([baron, baron, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.BaronDiscard(baron, player.PlayerState, player.Game.Kingdom), Times.Exactly(askedToDiscard));
		#endregion
	}
}
