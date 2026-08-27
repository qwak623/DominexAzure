using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerMillTests : CardWithPlayerTestsBase
{
	private readonly Card mill = Mill.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card estate = Estate.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(mill);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardsTwoForCoins()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mill, copper, silver, estate]);
		player.PlayerState.DrawPile = CreatePile([gold]);
		var millToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mill);

		user.Setup(u => u.MillWantsToDiscard(mill, player.PlayerState, player.Game.Kingdom)).Returns(true);
		user.Setup(u => u.MillChooseCardsToDiscard(mill, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>, int>((c, ps, k, cards, count) =>
				cards.Where(x => x.Card.Name == CardName.Copper || x.Card.Name == CardName.Silver).ToList());
		#endregion

		#region act
		player.PlayActionCardInternal(millToPlay);
		#endregion

		#region assert
		// +1 action, +1 card (mill's own), +$2 from discarding exactly two
		AssertNumbers(1, 2, 0, player);
		AssertPile([estate, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper, silver], player.PlayerState.DiscardPile);
		AssertPile([mill], player.PlayerState.CardsPlayed);
		AssertPile([mill], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.MillChooseCardsToDiscard(mill, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DeclinesToDiscard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mill, copper, silver]);
		player.PlayerState.DrawPile = CreatePile([gold]);
		var millToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mill);

		user.Setup(u => u.MillWantsToDiscard(mill, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(millToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper, silver, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mill], player.PlayerState.CardsPlayed);
		AssertPile([mill], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// declining means the player is never even asked which cards to discard
		user.Verify(u => u.MillChooseCardsToDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(),
			It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DoesNothingWhenHandIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mill]);
		var millToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(millToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mill], player.PlayerState.CardsPlayed);
		AssertPile([mill], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.MillWantsToDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DiscardsOneCardWithoutBonusWhenHandHasOnlyOneCard()
	{
		#region arrange
		// only one card is left in hand after mill's own draw - the player can still choose
		// to discard it, they just don't get the $2 bonus since it isn't a full pair
		player.PlayerState.Hand = CreatePile([mill, copper]);
		var millToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mill);

		user.Setup(u => u.MillWantsToDiscard(mill, player.PlayerState, player.Game.Kingdom)).Returns(true);
		user.Setup(u => u.MillChooseCardsToDiscard(mill, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>, int>((c, ps, k, cards, count) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(millToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper], player.PlayerState.DiscardPile);
		AssertPile([mill], player.PlayerState.CardsPlayed);
		AssertPile([mill], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.MillChooseCardsToDiscard(mill, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void CountsAsOneVictoryPoint()
	{
		Assert.AreEqual(1, mill.CountPoints(player));
	}
}
