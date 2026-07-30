using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Tests;

[TestClass]
public class PlayerTests
{
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private readonly Card village = Village.Get();
	private readonly Card militia = Militia.Get();

	private readonly Card moat = Moat.Get();

	private Player player;
	private Player player2;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		var kingdom = new Kingdom(new List<Card> { village }, 2); // todo should be mockable

		game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);
		game.Setup(g => g.Trash).Returns(new List<Card> { });
		user = new Mock<IUser>();
		player = new Player(game.Object, user.Object);
		player.PlayerState.DrawPile = new List<Card> { };

		var user2 = new Mock<IUser>();
		player2 = new Player(game.Object, user2.Object);
		game.Setup(g => g.Players).Returns(new List<IPlayer> { player, player2 });
	}

	[TestMethod]
	// TODO
	public void PlayTurn()
	{
		#region arrange

		#endregion

		#region act
		player.PlayTurn(5);
		#endregion

		#region assert
		Assert.Fail();
		#endregion
	}

	[TestMethod]
	public void PlayActionCard()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = new List<Card> { silver };
		player.PlayerState.Hand = new List<Card> { copper, silver, village, village };

		user.Setup(u => u.PlayCard(
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.All(card => card == village)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns(village);
		#endregion

		#region act
		Card playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Once);

		Assert.AreEqual(village, playedCard);

		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, village, silver }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.ActionsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DrawPile);
		Assert.AreEqual(2, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_NoActions()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.DrawPile = new List<Card> { silver };
		player.PlayerState.Hand = new List<Card> { copper, silver, village, village };
		#endregion

		#region act
		Card playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);

		Assert.IsNull(playedCard);

		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, village, village }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.ActionsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.DrawPile);
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_NoActionCardsInHand()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = new List<Card> { silver };
		player.PlayerState.Hand = new List<Card> { copper, silver, silver };
		#endregion

		#region act
		Card playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);
		Assert.IsNull(playedCard);

		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.ActionsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.DrawPile);
		Assert.AreEqual(1, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_DontPlayAnything()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = new List<Card> { silver };
		player.PlayerState.Hand = new List<Card> { copper, silver, village, village };

		user.Setup(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(), player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns<Card>(null);
		#endregion

		#region act
		Card playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.All(card => card == village)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Once);

		Assert.IsNull(playedCard);

		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, village, village }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.ActionsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.DrawPile);
		Assert.AreEqual(1, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}


	[TestMethod]
	public void PlayTreasure()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { copper, silver, silver, village };
		#endregion

		#region act
		player.PlayTreasure();
		#endregion

		#region assert
		Assert.AreEqual(5, player.PlayerState.Coins);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.ActionsPlayed);
		// TODO neměly by být v played? 
		//CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.PlayedCards);
		//CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Buy()
	{
		#region arrange
		player.PlayerState.Coins = 5;
		player.PlayerState.Buys = 1;

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => !k.OnlyTreasures && k.Price == 5),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns(village);
		#endregion

		#region act
		player.Buy();
		#endregion

		#region assert
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);

		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.DiscardPile);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void Buy_NoBuys()
	{
		#region arrange
		player.PlayerState.Coins = 5;
		player.PlayerState.Buys = 0;
		#endregion

		#region act
		player.Buy();
		#endregion

		#region assert
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);

		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		Assert.AreEqual(5, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void Buy_DontBuyAnything()
	{
		#region arrange
		player.PlayerState.Coins = 4;
		player.PlayerState.Buys = 1;

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => !k.OnlyTreasures && k.Price == 4),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns<Card>(null);
		#endregion

		#region act
		player.Buy();
		#endregion

		#region assert
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);

		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);
		#endregion
	}

	[TestMethod]
	public void Cleanup()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper, silver, village };
		player.PlayerState.Hand = new List<Card> { village, silver };
		player.PlayerState.DiscardPile = new List<Card> { village };
		player.PlayerState.CardsPlayed = new List<Card> { silver };
		player.PlayerState.ActionsPlayed = new List<Card> { };
		#endregion

		#region act
		player.Cleanup();
		#endregion

		#region assert
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.CardsPlayed.Any());
		Assert.IsFalse(player.PlayerState.ActionsPlayed.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, village }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { village, silver, village, silver }, player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void Draw()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper, silver, village };
		player.PlayerState.Hand = new List<Card> { village };
		#endregion

		#region act
		player.Draw(2);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, village, village }, player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Draw_Shuffle()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper };
		player.PlayerState.DiscardPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.Draw(2);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper }, player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Draw_NotEnoughCards()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper };
		player.PlayerState.DiscardPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.Draw(4);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper, silver }, player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Trash()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { copper, copper, copper, copper };
		game.Setup(g => g.Trash).Returns(new List<Card> { silver });
		#endregion

		#region act
		player.Trash(copper);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, copper }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, game.Object.Trash);
		#endregion
	}

	[TestMethod]
	public void Discard()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { copper, copper, copper, copper };
		player.PlayerState.DiscardPile = new List<Card> { silver };
		#endregion

		#region act
		player.Discard(copper);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, copper }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void Gain()
	{
		#region act
		for (int i = 0; i < 12; i++)
		{
			player.Gain(CardType.Village);
		}
		#endregion

		#region assert
		// only 10 villages are available in the kingdom
		CollectionAssert.AreEquivalent(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void GainToHand()
	{
		#region act
		for (int i = 0; i < 12; i++)
		{
			player.GainToHand(CardType.Village);
		}
		#endregion

		#region assert
		// only 10 villages are available in the kingdom
		CollectionAssert.AreEquivalent(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void GainToDrawPile()
	{
		#region act
		for (int i = 0; i < 12; i++)
		{
			player.GainToDrawPile(CardType.Village);
		}
		#endregion

		#region assert
		// only 10 villages are available in the kingdom
		CollectionAssert.AreEquivalent(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void ReturnToDrawPile()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { silver, silver };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.ReturnToDrawPile(silver);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void DiscardDrawPile()
	{
		#region arrange
		player.PlayerState.DiscardPile = new List<Card> { silver, silver };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.DiscardDrawPile();
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.DiscardPile);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void Show()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper, silver, village };
		#endregion

		#region act
		var shownCards = player.Show(2);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, village}, shownCards);
		#endregion
	}

	[TestMethod]
	public void Show_Shuffle()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper };
		player.PlayerState.DiscardPile = new List<Card> { silver, silver };
		#endregion

		#region act
		var shownCards = player.Show(2);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper }, shownCards);
		#endregion
	}

	[TestMethod]
	public void Show_NotEnoughCards()
	{
		#region arrange
		player.PlayerState.DrawPile = new List<Card> { copper };
		player.PlayerState.DiscardPile = new List<Card> { silver, silver };
		#endregion

		#region act
		var shownCards = player.Show(4);
		#endregion

		#region assert
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { }, player.PlayerState.DiscardPile);
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper, silver }, shownCards);
		#endregion
	}

	[TestMethod]
	public void DealAttack_Moat()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.Hand = new List<Card> { copper, copper, moat, copper, moat };

		user.SetupSequence(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(),
			player.PlayerState, player.Game.Kingdom, Phase.Reaction, militia))
			.Returns(moat).Returns((Card)null);
		#endregion

		#region act
		player.DealAttack(player2, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// user is asked to choose a reaction to play
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(),
			player.PlayerState, player.Game.Kingdom, Phase.Reaction, militia), Times.Exactly(2));

		// player blocked the attack and therefore his hand did not change
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, moat, copper, moat }, player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void DontReact_NoReactions()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.Hand = new List<Card> { copper, copper, copper, copper, copper };

		user.Setup(du => du.MilitiaDiscard(militia, player.PlayerState, player.Game.Kingdom, 2))
			.Returns(new List<Card> { copper, copper });
		#endregion

		#region act
		player.DealAttack(player2, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// user is never asked to choose a reaction to play because he does not have any
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<Card>>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);

		// user chose not to block the attack and therefore he needs to choose two cards to discard
		user.Verify(du => du.MilitiaDiscard(militia, player.PlayerState, player.Game.Kingdom, 2), Times.Once);

		// player discarded two cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, copper }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, player.PlayerState.DiscardPile);
		#endregion
	}
}