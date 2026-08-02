using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerCellarTests : CardWithPlayerTestsBase
{
	private readonly Card cellar = Cellar.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(cellar);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DrawNoCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([silver, silver, cellar, copper, silver]);
		var cellarToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Cellar);

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns([]);
		#endregion

		#region act
		player.PlayActionCardInternal(cellarToPlay);
		#endregion

		#region assert
		// nothing was discarded
		AssertNumbers(1, 0, 0, player);
		AssertPile([silver, silver, copper, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([cellar], player.PlayerState.CardsPlayed);
		AssertPile([cellar], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DrawOneCard()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([gold]);
		player.PlayerState.Hand = CreatePile([silver, silver, cellar, copper, silver]);
		var cellarToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Cellar);

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns([player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper)]);
		#endregion

		#region act
		player.PlayActionCardInternal(cellarToPlay);
		#endregion

		#region assert
		// the copper was discarded
		// the player draws one card - a gold
		AssertNumbers(1, 0, 0, player);
		AssertPile([silver, silver, silver, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper], player.PlayerState.DiscardPile);
		AssertPile([cellar], player.PlayerState.CardsPlayed);
		AssertPile([cellar], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DrawFourCards()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([gold, gold, gold, gold]);
		player.PlayerState.Hand = CreatePile([silver, copper, cellar, copper, cellar]);
		var cellarToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Cellar);

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns([.. player.PlayerState.Hand.Where(c => c != cellarToPlay)]);
		#endregion

		#region act
		player.PlayActionCardInternal(cellarToPlay);
		#endregion

		#region assert
		// the cards were discarded
		// the player draws four cards
		AssertNumbers(1, 0, 0, player);
		AssertPile([gold, gold, gold, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper, cellar, copper, silver], player.PlayerState.DiscardPile);
		AssertPile([cellar], player.PlayerState.CardsPlayed);
		AssertPile([cellar], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DrawTheSameCardBack()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([silver, copper, cellar, copper, cellar]);
		var cellarToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Cellar);

		user.Setup(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns([player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper)]);
		#endregion

		#region act
		player.PlayActionCardInternal(cellarToPlay);
		#endregion

		#region assert
		// the card was discarded, but it was mixed to the draw pile afterwards
		// the player draws the same copper back
		// nothing stayed on the draw pile
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper, silver, copper, cellar], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([cellar], player.PlayerState.CardsPlayed);
		AssertPile([cellar], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to discard
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([copper, copper, silver, cellar, throneRoom]);
		player.PlayerState.DrawPile = CreatePile([gold, gold, copper]);
		var throneRoomToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom);
		var cellarToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Cellar);
		user.SetupSequence(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom))
			.Returns([.. player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper)])
			// evaluated lazily: the copper the second resolution discards is the one the first
			// resolution just drew back into hand, which doesn't exist yet at arrange time
			.Returns(() => [player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper)]);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Contains(cellarToPlay)))).Returns(cellarToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoomToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([gold, gold, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper, copper, copper], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, cellar], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, cellar, cellar], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to discard two times
		user.Verify(u => u.CellarDiscard(cellar, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}
}