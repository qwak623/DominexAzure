using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerTradingPostTests : CardWithPlayerTestsBase
{
	private readonly Card tradingPost = TradingPost.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(tradingPost);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void AutoTrashesBothCardsWhenHandHasExactlyTwo()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tradingPost, copper, estate]);
		var tradingPostToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.TradingPost);
		#endregion

		#region act
		player.PlayActionCardInternal(tradingPostToPlay);
		#endregion

		#region assert
		// hand drops to exactly 2 once trading post itself is played, so both are trashed
		// automatically - no choice to make - and silver goes straight into the hand
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tradingPost], player.PlayerState.CardsPlayed);
		AssertPile([tradingPost], player.PlayerState.ActionsPlayed);
		AssertPile([copper, estate], player.Game.Trash);

		user.Verify(u => u.TradingPostTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AutoTrashesSingleCardWithoutGainingSilver()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tradingPost, copper]);
		var tradingPostToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.TradingPost);
		#endregion

		#region act
		player.PlayActionCardInternal(tradingPostToPlay);
		#endregion

		#region assert
		// only 1 card trashed - fewer than 2, so no silver
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tradingPost], player.PlayerState.CardsPlayed);
		AssertPile([tradingPost], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);

		user.Verify(u => u.TradingPostTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DoesNothingWhenHandIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tradingPost]);
		var tradingPostToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(tradingPostToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tradingPost], player.PlayerState.CardsPlayed);
		AssertPile([tradingPost], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.TradingPostTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AsksUserAndGainsSilverWhenTwoAreChosen()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tradingPost, copper, copper, estate]);
		var tradingPostToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.TradingPost);
		var coppersToTrash = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).ToList();

		user.Setup(u => u.TradingPostTrash(tradingPost, player.PlayerState, player.Game.Kingdom))
			.Returns(coppersToTrash);
		#endregion

		#region act
		player.PlayActionCardInternal(tradingPostToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([estate, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tradingPost], player.PlayerState.CardsPlayed);
		AssertPile([tradingPost], player.PlayerState.ActionsPlayed);
		AssertPile([copper, copper], player.Game.Trash);

		user.Verify(u => u.TradingPostTrash(tradingPost, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomSecondResolutionAutoTrashesShrunkenHand()
	{
		#region arrange
		// first resolution starts with 3 cards (>2, has to ask); trashing 2 and gaining a
		// silver to hand leaves exactly 2 cards, so the second resolution auto-trashes both
		// (including the silver just gained) without asking, and gains yet another silver
		player.PlayerState.Hand = CreatePile([throneRoom, tradingPost, copper, copper, copper]);
		var tradingPostToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.TradingPost);
		var coppersInHand = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).ToList();

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == tradingPostToPlay))).Returns(tradingPostToPlay);
		user.Setup(u => u.TradingPostTrash(tradingPost, player.PlayerState, player.Game.Kingdom))
			.Returns([coppersInHand[0], coppersInHand[1]]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tradingPost, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([tradingPost, tradingPost, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([copper, copper, copper, silver], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		// only the first resolution needs to ask - the second's hand is small enough to
		// auto-trash
		user.Verify(u => u.TradingPostTrash(tradingPost, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}
}
