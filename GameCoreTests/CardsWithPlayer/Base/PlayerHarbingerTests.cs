using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerHarbingerTests : CardWithPlayerTestsBase
{
	private readonly Card harbinger = Harbinger.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(harbinger);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PutsTheChosenDiscardedCardOntoTheDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([harbinger]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver, gold]);
		var harbingerToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.HarbingerPutOnTop(harbinger, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2))).Returns<Card, PlayerState, Kingdom, List<CardInstance>>(
			(c, ps, k, cards) => cards.Single(x => x.Card.Name == CardName.Gold));
		#endregion

		#region act
		player.PlayActionCardInternal(harbingerToPlay);
		#endregion

		#region assert
		// +1 Card and +1 Action cancel out playing harbinger itself
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([gold], player.PlayerState.DrawPile);
		AssertPile([silver], player.PlayerState.DiscardPile);
		AssertPile([harbinger], player.PlayerState.CardsPlayed);
		AssertPile([harbinger], player.PlayerState.ActionsPlayed);

		// the whole discard pile is looked through, not just the top of it
		user.Verify(u => u.HarbingerPutOnTop(harbinger, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void MayDeclineToPutAnythingBack()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([harbinger]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver]);
		var harbingerToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.HarbingerPutOnTop(harbinger, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(harbingerToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([silver], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void NothingHappensWhenDiscardPileIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([harbinger]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([]);
		var harbingerToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(harbingerToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);

		user.Verify(u => u.HarbingerPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
