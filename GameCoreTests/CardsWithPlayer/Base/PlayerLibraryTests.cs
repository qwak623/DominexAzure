using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerLibraryTests : CardWithPlayerTestsBase
{
	private readonly Card library = Library.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card province = Province.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { library, adventurer });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void AlreadyHas7Cards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([copper, copper, library, adventurer, silver, silver, gold, province]);
		var libraryToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Library);
		#endregion

		#region act
		player.PlayActionCardInternal(libraryToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, adventurer, silver, silver, gold, province], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([library], player.PlayerState.CardsPlayed);
		AssertPile([library], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// hand is already at 7, so the loop never runs and nothing is ever shown
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<CardInstance>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper, silver, library]);
		player.PlayerState.DrawPile = CreatePile([library, adventurer, copper]);
		var libraryToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Library);
		var adventurerShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Adventurer);
		var libraryShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Library);

		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(libraryToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([adventurer, copper, silver, copper, adventurer], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);

		// the library shown from the draw pile - a different physical card than the one being
		// played - gets skipped and discarded rather than drawn
		AssertPile([library], player.PlayerState.DiscardPile);
		AssertPile([library], player.PlayerState.CardsPlayed);
		AssertPile([library], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown), Times.Once);

		// only action cards are offered as a skip choice
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.Is<CardInstance>(c => !c.IsAction)), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDrawToSevenCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper, silver, library, throneRoom]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, library, adventurer, copper]);
		var libraryToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Library);
		var adventurerShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Adventurer);
		var libraryShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Library);

		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown)).Returns(true);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Contains(libraryToPlay)))).Returns(libraryToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([adventurer, copper, silver, copper, adventurer, silver, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([library], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, library], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, library, library], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the first resolution fills the hand to exactly 7, so the second resolution's loop
		// condition is false from the start and never shows anything
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNotEnoughCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper, silver, library, throneRoom]);
		player.PlayerState.DrawPile = CreatePile([copper, throneRoom, library, adventurer]);
		var libraryToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Library);
		var adventurerShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Adventurer);
		var libraryShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Library);
		var throneRoomShown = player.PlayerState.DrawPile.First(c => c.Card.Type == CardType.ThroneRoom);

		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown)).Returns(false);
		user.Setup(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown)).Returns(true);
		user.SetupSequence(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, throneRoomShown))
			.Returns(true).Returns(false);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Contains(libraryToPlay)))).Returns(libraryToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// the first resolution's draw pile runs out after setting library and throne room aside;
		// those get discarded at the end of that resolution, so when the second resolution's
		// draw pile is also empty, ShuffleIfNeeded reshuffles them straight back in and shows
		// them again - library is skipped a second time, throne room isn't and gets drawn
		AssertNumbers(0, 0, 0, player);
		AssertPile([adventurer, copper, silver, adventurer, copper, throneRoom], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([library], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, library], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, library, library], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, adventurerShown), Times.Once);
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, libraryShown), Times.Exactly(2));
		user.Verify(u => u.LibrarySkip(library, player.PlayerState, player.Game.Kingdom, throneRoomShown), Times.Exactly(2));
		user.Verify(u => u.LibrarySkip(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.Is<CardInstance>(c => !c.IsAction)), Times.Never);
		#endregion
	}
}
