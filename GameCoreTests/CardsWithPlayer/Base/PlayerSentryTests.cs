using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerSentryTests : CardWithPlayerTestsBase
{
	private readonly Card sentry = Sentry.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(sentry);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void NothingIsAskedWhenDrawPileIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		player.PlayerState.DrawPile = CreatePile([]);
		var sentryToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		// +1 action cancels out playing sentry itself; the +1 card draw finds nothing to draw
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);
		AssertPile([sentry], player.PlayerState.CardsPlayed);
		AssertPile([sentry], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.SentryTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.SentryDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.SentryOrderCards(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void TrashesBothRevealedCardsAndSkipsDiscardAndOrder()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		// gold is drawn immediately by sentry's own +1 Card; silver and copper are what's left
		// on top of the deck for the "look at the top 2 cards" effect to reveal
		player.PlayerState.DrawPile = CreatePile([copper, silver, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, copper], player.Game.Trash);

		user.Verify(u => u.SentryDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.SentryOrderCards(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DiscardsBothRevealedCardsAndSkipsOrder()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryDiscard(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([silver, copper], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.SentryOrderCards(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void TrashesOneAndDiscardsTheOther()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => [cards.Single(x => x.Card.Name == CardName.Copper)]);
		// only silver remains once copper was trashed
		user.Setup(u => u.SentryDiscard(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 1)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([silver], player.PlayerState.DiscardPile);
		AssertPile([copper], player.Game.Trash);

		user.Verify(u => u.SentryOrderCards(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void OrdersBothRevealedCardsWhenNothingIsTrashedOrDiscarded()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryDiscard(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryOrderCards(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 2)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([silver, copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.SentryOrderCards(sentry, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AppliesTheChosenOrderToTheTopOfTheDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		// gold is drawn away first; silver is then on top, with copper underneath it
		player.PlayerState.DrawPile = CreatePile([copper, silver, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryDiscard(sentry, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(new List<CardInstance>());
		// put silver back first (bottom) and copper back last (top), flipping their relative order
		user.Setup(u => u.SentryOrderCards(sentry, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
				[cards.Single(x => x.Card.Name == CardName.Silver), cards.Single(x => x.Card.Name == CardName.Copper)]);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		player.Draw(1);
		#endregion

		#region assert
		// copper was placed last in the chosen order, i.e. on top of the deck, even though silver
		// was the card actually on top before sentry looked at the deck
		AssertPile([gold, copper], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void RevealsOnlyOneCardWhenDeckIsSmall()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([sentry]);
		// gold is drawn away by sentry's own +1 Card, leaving only copper to look at
		player.PlayerState.DrawPile = CreatePile([copper, gold]);
		var sentryToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SentryTrash(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 1)))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryDiscard(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 1)))
			.Returns(new List<CardInstance>());
		user.Setup(u => u.SentryOrderCards(sentry, player.PlayerState, player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Count == 1)))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(sentryToPlay);
		#endregion

		#region assert
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);

		user.Verify(u => u.SentryTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.Is<List<CardInstance>>(c => c.Count == 1)), Times.Once);
		user.Verify(u => u.SentryDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.Is<List<CardInstance>>(c => c.Count == 1)), Times.Once);
		user.Verify(u => u.SentryOrderCards(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.Is<List<CardInstance>>(c => c.Count == 1)), Times.Once);
		#endregion
	}
}
