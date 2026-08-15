using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerSpyTests : CardWithPlayerTestsBase
{
	private readonly Card spy = Spy.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card province = Province.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player1;
	private Player player2;
	private Player player3;
	private Player player4;

	private Mock<IUser> user1;
	private Mock<IUser> user2;
	private Mock<IUser> user3;
	private Mock<IUser> user4;

	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(spy);

		user1 = new Mock<IUser>();
		user2 = new Mock<IUser>();
		user3 = new Mock<IUser>();
		user4 = new Mock<IUser>();

		player1 = CreatePlayer(game.Object, user1.Object);
		player2 = CreatePlayer(game.Object, user2.Object);
		player3 = CreatePlayer(game.Object, user3.Object);
		player4 = CreatePlayer(game.Object, user4.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { player2, player1, player3, player4 });
	}

	[TestMethod]
	public void PlayerDiscardsCard()
	{
		#region arrange
		player1.PlayerState.Hand = CreatePile([spy]);
		player1.PlayerState.DrawPile = CreatePile([province, duchy]);
		var spyToPlay = player1.PlayerState.Hand[0];

		player2.PlayerState.DrawPile = CreatePile([province]);
		player3.PlayerState.DrawPile = CreatePile([province]);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Action))
			.Returns(true);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Attack))
			.Returns(true).Returns(false);
		#endregion

		#region act
		player1.PlayActionCardInternal(spyToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player1);
		AssertPile([duchy], player1.PlayerState.Hand);
		AssertPile([], player1.PlayerState.DrawPile);
		AssertPile([province], player1.PlayerState.DiscardPile);
		AssertPile([spy], player1.PlayerState.CardsPlayed);
		AssertPile([spy], player1.PlayerState.ActionsPlayed);
		AssertPile([], player1.Game.Trash);

		// player2's card is discarded
		AssertNumbers(1, 0, 0, player2);
		AssertPile([], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([province], player2.PlayerState.DiscardPile);
		AssertPile([], player2.PlayerState.CardsPlayed);
		AssertPile([], player2.PlayerState.ActionsPlayed);

		// player3's card is put back
		AssertNumbers(1, 0, 0, player3);
		AssertPile([], player3.PlayerState.Hand);
		AssertPile([province], player3.PlayerState.DrawPile);
		AssertPile([], player3.PlayerState.DiscardPile);
		AssertPile([], player3.PlayerState.CardsPlayed);
		AssertPile([], player3.PlayerState.ActionsPlayed);

		// player4 has nothing to show
		AssertNumbers(1, 0, 0, player4);
		AssertPile([], player4.PlayerState.Hand);
		AssertPile([], player4.PlayerState.DrawPile);
		AssertPile([], player4.PlayerState.DiscardPile);
		AssertPile([], player4.PlayerState.CardsPlayed);
		AssertPile([], player4.PlayerState.ActionsPlayed);

		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Action), Times.Once);
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Attack), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntDiscardCard()
	{
		#region arrange
		player1.PlayerState.Hand = CreatePile([spy]);
		player1.PlayerState.DrawPile = CreatePile([silver, duchy]);
		var spyToPlay = player1.PlayerState.Hand[0];

		player2.PlayerState.DrawPile = CreatePile([province]);
		player4.PlayerState.DrawPile = CreatePile([province]);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Silver), Phase.Action))
			.Returns(false);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Attack))
			.Returns(false).Returns(true);
		#endregion

		#region act
		player1.PlayActionCardInternal(spyToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player1);
		AssertPile([duchy], player1.PlayerState.Hand);
		AssertPile([silver], player1.PlayerState.DrawPile);
		AssertPile([], player1.PlayerState.DiscardPile);
		AssertPile([spy], player1.PlayerState.CardsPlayed);
		AssertPile([spy], player1.PlayerState.ActionsPlayed);
		AssertPile([], player1.Game.Trash);

		// player2's card is put back
		AssertNumbers(1, 0, 0, player2);
		AssertPile([], player2.PlayerState.Hand);
		AssertPile([province], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);
		AssertPile([], player2.PlayerState.CardsPlayed);
		AssertPile([], player2.PlayerState.ActionsPlayed);

		// player3 has nothing to show
		AssertNumbers(1, 0, 0, player3);
		AssertPile([], player3.PlayerState.Hand);
		AssertPile([], player3.PlayerState.DrawPile);
		AssertPile([], player3.PlayerState.DiscardPile);
		AssertPile([], player3.PlayerState.CardsPlayed);
		AssertPile([], player3.PlayerState.ActionsPlayed);

		// player4's card is discarded
		AssertNumbers(1, 0, 0, player4);
		AssertPile([], player4.PlayerState.Hand);
		AssertPile([], player4.PlayerState.DrawPile);
		AssertPile([province], player4.PlayerState.DiscardPile);
		AssertPile([], player4.PlayerState.CardsPlayed);
		AssertPile([], player4.PlayerState.ActionsPlayed);

		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Silver), Phase.Action), Times.Once);
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.Is<CardInstance>(c => c.Card.Type == CardType.Province), Phase.Attack), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public void ThroneRoomPlay(bool discardFirstTime, bool discardSecondTime)
	{
		#region arrange
		player1.PlayerState.Hand = CreatePile([throneRoom, spy]);
		player1.PlayerState.DrawPile = CreatePile([province, silver, spy, duchy]);
		player2.PlayerState.DrawPile = CreatePile([province, silver]);
		player3.PlayerState.DrawPile = CreatePile([province]);
		player4.PlayerState.DrawPile = CreatePile([]);
		var spyToPlay = player1.PlayerState.Hand.First(c => c.Card.Type == CardType.Spy);

		user1.Setup(u => u.ThroneRoomPlay(throneRoom, player1.PlayerState,
			player1.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == spyToPlay))).Returns(spyToPlay);

		user1.SetupSequence(du => du.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, It.IsAny<CardInstance>(), Phase.Action))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		user1.SetupSequence(du => du.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, It.IsAny<CardInstance>(), Phase.Attack))
			.Returns(discardFirstTime).Returns(discardFirstTime).Returns(discardSecondTime).Returns(discardSecondTime);
		#endregion

		#region act
		player1.PlayActionCardInternal(player1.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player1);

		switch ((discardFirstTime, discardSecondTime))
		{
			case (true, true):
				AssertPile([duchy, silver], player1.PlayerState.Hand);
				AssertPile([], player1.PlayerState.DrawPile);
				AssertPile([spy, province], player1.PlayerState.DiscardPile);

				AssertPile([], player2.PlayerState.DrawPile);
				AssertPile([silver, province], player2.PlayerState.DiscardPile);

				AssertPile([], player3.PlayerState.DrawPile);
				AssertPile([province], player3.PlayerState.DiscardPile);
				break;
			case (true, false):
				AssertPile([duchy, silver], player1.PlayerState.Hand);
				AssertPile([province], player1.PlayerState.DrawPile);
				AssertPile([spy], player1.PlayerState.DiscardPile);

				AssertPile([province], player2.PlayerState.DrawPile);
				AssertPile([silver], player2.PlayerState.DiscardPile);

				AssertPile([province], player3.PlayerState.DrawPile);
				AssertPile([], player3.PlayerState.DiscardPile);
				break;
			case (false, true):
				AssertPile([duchy, spy], player1.PlayerState.Hand);
				AssertPile([province], player1.PlayerState.DrawPile);
				AssertPile([silver], player1.PlayerState.DiscardPile);

				AssertPile([province], player2.PlayerState.DrawPile);
				AssertPile([silver], player2.PlayerState.DiscardPile);

				AssertPile([], player3.PlayerState.DrawPile);
				AssertPile([province], player3.PlayerState.DiscardPile);
				break;
			case (false, false):
				AssertPile([duchy, spy], player1.PlayerState.Hand);
				AssertPile([province, silver], player1.PlayerState.DrawPile);
				AssertPile([], player1.PlayerState.DiscardPile);

				AssertPile([province, silver], player2.PlayerState.DrawPile);
				AssertPile([], player2.PlayerState.DiscardPile);

				AssertPile([province], player3.PlayerState.DrawPile);
				AssertPile([], player3.PlayerState.DiscardPile);
				break;
		}
		AssertPile([spy, throneRoom], player1.PlayerState.CardsPlayed);
		AssertPile([spy, spy, throneRoom], player1.PlayerState.ActionsPlayed);
		AssertPile([], player1.Game.Trash);

		// player2's and player3's hands never changed - only their draw/discard piles did, above
		AssertNumbers(1, 0, 0, player2);
		AssertPile([], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.CardsPlayed);
		AssertPile([], player2.PlayerState.ActionsPlayed);

		AssertNumbers(1, 0, 0, player3);
		AssertPile([], player3.PlayerState.Hand);
		AssertPile([], player3.PlayerState.CardsPlayed);
		AssertPile([], player3.PlayerState.ActionsPlayed);

		// player4 never had anything to show
		AssertNumbers(1, 0, 0, player4);
		AssertPile([], player4.PlayerState.Hand);
		AssertPile([], player4.PlayerState.DrawPile);
		AssertPile([], player4.PlayerState.DiscardPile);
		AssertPile([], player4.PlayerState.CardsPlayed);
		AssertPile([], player4.PlayerState.ActionsPlayed);

		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.IsAny<CardInstance>(), Phase.Action), Times.Exactly(2));

		// attack fires once per throne-room resolution against each of the 3 other players
		// (player4 never has anything to show). A discard pile gets reshuffled back into the
		// draw pile once it's empty (ShuffleIfNeeded, inside Show), so a player can legitimately
		// be asked twice even starting with a single card - see player3 across all four cases.
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.IsAny<CardInstance>(), Phase.Attack), Times.Exactly(4));
		#endregion
	}
}
