using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using Moq;

namespace GameCore.Tests;

[TestClass]
public class PlayerTests
{
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card estate = Estate.Get();

	private readonly Card village = Village.Get();
	private readonly Card militia = Militia.Get();

	private readonly Card moat = Moat.Get();

	private Player player;
	private Player player2;

	private Mock<IUser> user;
	private Mock<IGame> game;
	private Kingdom kingdom;

	[TestInitialize]
	public void Init()
	{
		kingdom = new Kingdom([village], 2); // todo should be mockable

		game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);
		game.Setup(g => g.Trash).Returns(new Pile { });
		user = new Mock<IUser>();
		player = new Player(game.Object, user.Object);
		player.PlayerState.DrawPile = new Pile { };

		var user2 = new Mock<IUser>();
		player2 = new Player(game.Object, user2.Object);
		game.Setup(g => g.Players).Returns([player, player2]);
	}

	[TestMethod]
	public void PlayTurn()
	{
		#region arrange
		// hand: one action (village) and one treasure (copper) already in hand.
		// draw pile: silver is on top so village's own +1 Card draws it, leaving hand with no
		// more action cards and ending the action phase after a single play. the remaining 5
		// cards are exactly drawCount, so the end-of-turn draw takes the whole pile.
		player.PlayerState.Hand = CreatePile([village, copper]);
		player.PlayerState.DrawPile = CreatePile([gold, copper, copper, copper, copper, silver]);
		var villageToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Village);

		user.Setup(u => u.PlayCard(
			It.Is<IEnumerable<CardInstance>>(c => c.Single() == villageToPlay),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns(villageToPlay);

		// coins after playing copper + the drawn silver: 1 + 2 = 3
		var estateToGain = player.Game.Kingdom.GetPile(CardType.Estate).CardInstance;
		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 3 && !k.OnlyTreasures),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns(estateToGain);
		#endregion

		#region act
		player.PlayTurn(5);
		#endregion

		#region assert
		// -1 Action (played village) +2 Actions (village) = 2; 3 coins - 2 (estate price) = 1; -1 buy
		AssertNumbers(2, 1, 0, player);

		AssertPile([gold, copper, copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([village], player.PlayerState.ActionsPlayed);

		// cleanup moves the played village and the (never-moved) played treasures to discard,
		// on top of the estate gained during the buy phase
		AssertPile([estate, copper, silver, village], player.PlayerState.DiscardPile);

		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayTurn_NoActionCards()
	{
		#region arrange
		// no action cards in hand at all, so the action phase never even asks PlayCard
		player.PlayerState.Hand = CreatePile([copper, silver]);
		player.PlayerState.DrawPile = CreatePile([gold, copper, copper, copper, copper]);

		// coins from playing copper + silver: 1 + 2 = 3
		var estateToGain = player.Game.Kingdom.GetPile(CardType.Estate).CardInstance;
		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 3 && !k.OnlyTreasures),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns(estateToGain);
		#endregion

		#region act
		player.PlayTurn(5);
		#endregion

		#region assert
		// no action played, so actions stay at the phase-start reset value; 3 coins - 2 (estate)
		AssertNumbers(1, 1, 0, player);

		AssertPile([gold, copper, copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([estate, copper, silver], player.PlayerState.DiscardPile);

		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayTurn_MultipleActionCards()
	{
		#region arrange
		// village (+2 Actions, +1 Card) is played first, leaving enough actions to also play
		// moat (+0 Actions, +2 Cards) - the action phase should loop twice, not just once
		player.PlayerState.Hand = CreatePile([village, moat, copper]);
		player.PlayerState.DrawPile = CreatePile([gold, copper, copper, copper, copper, copper, silver, silver]);
		var villageToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Village);
		var moatToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Moat);

		user.Setup(u => u.PlayCard(
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 2 && c.Contains(villageToPlay) && c.Contains(moatToPlay)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns(villageToPlay);
		user.Setup(u => u.PlayCard(
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 1 && c.Contains(moatToPlay)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns(moatToPlay);

		// coins from playing copper + the two drawn silvers: 1 + 2 + 2 + 1 (drawn copper) = 6
		var estateToGain = player.Game.Kingdom.GetPile(CardType.Estate).CardInstance;
		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 6 && !k.OnlyTreasures),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns(estateToGain);
		#endregion

		#region act
		player.PlayTurn(5);
		#endregion

		#region assert
		// -1 (village) +2 (village) -1 (moat) +0 (moat) = 1 action left over; 6 - 2 (estate) = 4
		AssertNumbers(1, 4, 0, player);

		AssertPile([gold, copper, copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([village, moat], player.PlayerState.ActionsPlayed);
		AssertPile([estate, copper, silver, silver, copper, village, moat], player.PlayerState.DiscardPile);

		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Exactly(2));
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayTurn_NothingWorthBuying()
	{
		#region arrange
		// a single treasure and nothing the user wants to buy with it - the buy is asked about
		// exactly once and Buys is never spent, since Buy() only decrements it on a real gain
		player.PlayerState.Hand = CreatePile([copper]);
		player.PlayerState.DrawPile = CreatePile([gold, copper, copper, copper, copper]);

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 1 && !k.OnlyTreasures),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayTurn(5);
		#endregion

		#region assert
		AssertNumbers(1, 1, 1, player);

		AssertPile([gold, copper, copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.PlayerState.DiscardPile);

		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = CreatePile([silver]);
		player.PlayerState.Hand = CreatePile([copper, silver, village, village]);
		var villageToPlay = player.PlayerState.Hand.First(c => c.Card == village);

		user.Setup(u => u.PlayCard(
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 2 && c.All(card => card.Card == village)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null)).Returns(villageToPlay);
		#endregion

		#region act
		CardInstance playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Once);

		Assert.AreEqual(village, playedCard.Card);

		AssertPile([copper, silver, village, silver], player.PlayerState.Hand);
		AssertPile([village], player.PlayerState.CardsPlayed);
		AssertPile([village], player.PlayerState.ActionsPlayed);
		AssertPile([], player.PlayerState.DrawPile);
		AssertNumbers(2, 0, 0, player);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_NoActions()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.DrawPile = CreatePile([silver]);
		player.PlayerState.Hand = CreatePile([copper, silver, village, village]);
		#endregion

		#region act
		CardInstance playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);

		Assert.IsNull(playedCard);

		AssertPile([copper, silver, village, village], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertNumbers(0, 0, 0, player);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_NoActionCardsInHand()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = CreatePile([silver]);
		player.PlayerState.Hand = CreatePile([copper, silver, silver]);
		#endregion

		#region act
		CardInstance playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);
		Assert.IsNull(playedCard);

		AssertPile([copper, silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertNumbers(1, 0, 0, player);
		#endregion
	}

	[TestMethod]
	public void PlayActionCard_DontPlayAnything()
	{
		#region arrange
		player.PlayerState.Actions = 1;
		player.PlayerState.DrawPile = CreatePile([silver]);
		player.PlayerState.Hand = CreatePile([copper, silver, village, village]);

		user.Setup(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(), player.PlayerState,
			game.Object.Kingdom, Phase.Action, null)).Returns((CardInstance)null);
		#endregion

		#region act
		CardInstance playedCard = player.PlayActionCard();
		#endregion

		#region assert
		user.Verify(u => u.PlayCard(
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 2 && c.All(card => card.Card == village)),
			player.PlayerState, game.Object.Kingdom, Phase.Action, null), Times.Once);

		Assert.IsNull(playedCard);

		AssertPile([copper, silver, village, village], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertNumbers(1, 0, 0, player);
		#endregion
	}

	[TestMethod]
	public void PlayTreasure()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([copper, silver, silver, village]);
		#endregion

		#region act
		player.PlayTreasure();
		#endregion

		#region assert
		AssertNumbers(0, 5, 0, player);
		AssertPile([], player.PlayerState.ActionsPlayed);
		// TODO neměly by být v played?
		//AssertPile([copper, silver, silver], player.PlayerState.CardsPlayed);
		//AssertPile([village], player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Buy()
	{
		#region arrange
		player.PlayerState.Coins = 5;
		player.PlayerState.Buys = 1;
		var villageToGain = player.Game.Kingdom.GetPile(CardType.Village).CardInstance;

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => !k.OnlyTreasures && k.Price == 5),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns(villageToGain);
		#endregion

		#region act
		player.Buy();
		#endregion

		#region assert
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);

		AssertPile([village], player.PlayerState.DiscardPile);
		AssertNumbers(0, 2, 0, player);
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

		AssertPile([], player.PlayerState.DiscardPile);
		AssertNumbers(0, 5, 0, player);
		#endregion
	}

	[TestMethod]
	public void Buy_DontBuyAnything()
	{
		#region arrange
		player.PlayerState.Coins = 4;
		player.PlayerState.Buys = 1;

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => !k.OnlyTreasures && k.Price == 4),
			player.PlayerState, game.Object.Kingdom, Phase.Buy)).Returns((CardInstance)null);
		#endregion

		#region act
		player.Buy();
		#endregion

		#region assert
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			player.PlayerState, game.Object.Kingdom, Phase.Buy), Times.Once);

		AssertPile([], player.PlayerState.DiscardPile);
		AssertNumbers(0, 4, 1, player);
		#endregion
	}

	[TestMethod]
	public void Cleanup()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper, silver, village]);
		player.PlayerState.Hand = CreatePile([village, silver]);
		player.PlayerState.DiscardPile = CreatePile([village]);
		player.PlayerState.CardsPlayed = CreatePile([silver]);
		player.PlayerState.ActionsPlayed = [];
		#endregion

		#region act
		player.Cleanup();
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([], player.PlayerState.ActionsPlayed);
		AssertPile([copper, silver, village], player.PlayerState.DrawPile);
		AssertPile([village, silver, village, silver], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void Draw()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper, silver, village]);
		player.PlayerState.Hand = CreatePile([village]);
		#endregion

		#region act
		player.Draw(2);
		#endregion

		#region assert
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([silver, village, village], player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Draw_Shuffle()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver, silver]);
		#endregion

		#region act
		player.Draw(2);
		#endregion

		#region assert
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, copper], player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Draw_NotEnoughCards()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver, silver]);
		#endregion

		#region act
		player.Draw(4);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, copper, silver], player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void Trash()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([copper, copper, copper, copper]);
		var copperToTrash = player.PlayerState.Hand[0];
		game.Setup(g => g.Trash).Returns(CreatePile([silver]));
		#endregion

		#region act
		player.Trash(copperToTrash);
		#endregion

		#region assert
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([copper, silver], game.Object.Trash);
		#endregion
	}

	[TestMethod]
	public void Discard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([copper, copper, copper, copper]);
		var copperToDiscard = player.PlayerState.Hand[0];
		player.PlayerState.DiscardPile = CreatePile([silver]);
		#endregion

		#region act
		player.Discard(copperToDiscard);
		#endregion

		#region assert
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([copper, silver], player.PlayerState.DiscardPile);
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
		AssertPile(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.DiscardPile);
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
		AssertPile(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.Hand);
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
		AssertPile(Enumerable.Repeat(village, 10).ToList(), player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void ReturnToDrawPile()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([silver, silver]);
		var silverToReturn = player.PlayerState.Hand[0];
		player.PlayerState.DrawPile = CreatePile([copper]);
		#endregion

		#region act
		player.ReturnToDrawPile(silverToReturn);
		#endregion

		#region assert
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([copper, silver], player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void Show()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper, silver, village]);
		#endregion

		#region act
		var shownCards = player.Show(2);
		#endregion

		#region assert
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([silver, village], shownCards);
		#endregion
	}

	[TestMethod]
	public void Show_Shuffle()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver, silver]);
		#endregion

		#region act
		var shownCards = player.Show(2);
		#endregion

		#region assert
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, copper], shownCards);
		#endregion
	}

	[TestMethod]
	public void Show_NotEnoughCards()
	{
		#region arrange
		player.PlayerState.DrawPile = CreatePile([copper]);
		player.PlayerState.DiscardPile = CreatePile([silver, silver]);
		#endregion

		#region act
		var shownCards = player.Show(4);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, copper, silver], shownCards);
		#endregion
	}

	[TestMethod]
	public void DealAttack_Moat()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.Hand = CreatePile([copper, copper, moat, copper, moat]);
		var moats = player.PlayerState.Hand.Where(c => c.Card == moat).ToList();

		user.SetupSequence(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			player.PlayerState, player.Game.Kingdom, Phase.Reaction, militia))
			.Returns(moats[0]).Returns((CardInstance)null);
		#endregion

		#region act
		player.DealAttack(player2, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		AssertNumbers(0, 0, 0, player);

		// user is asked to choose a reaction to play
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			player.PlayerState, player.Game.Kingdom, Phase.Reaction, militia), Times.Exactly(2));

		// player blocked the attack and therefore his hand did not change
		AssertPile([copper, copper, moat, copper, moat], player.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void DontReact_NoReactions()
	{
		#region arrange
		player.PlayerState.Actions = 0;
		player.PlayerState.Hand = CreatePile([copper, copper, copper, copper, copper]);
		var coppersToDiscard = player.PlayerState.Hand.Take(2).ToList();

		user.Setup(du => du.MilitiaDiscard(militia, player.PlayerState, player.Game.Kingdom, 2))
			.Returns(coppersToDiscard);
		#endregion

		#region act
		player.DealAttack(player2, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		AssertNumbers(0, 0, 0, player);

		// user is never asked to choose a reaction to play because he does not have any
		user.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>(), It.IsAny<Card>()), Times.Never);

		// user chose not to block the attack and therefore he needs to choose two cards to discard
		user.Verify(du => du.MilitiaDiscard(militia, player.PlayerState, player.Game.Kingdom, 2), Times.Once);

		// player discarded two cards
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([copper, copper], player.PlayerState.DiscardPile);
		#endregion
	}

	private int nextCardId = 1000; // kept away from the real Kingdom's own id range to avoid collisions

	private void AssertNumbers(int expectedActions, int expectedCoins, int expectedBuys, Player player)
	{
		Assert.AreEqual(expectedActions, player.PlayerState.Actions);
		Assert.AreEqual(expectedCoins, player.PlayerState.Coins);
		Assert.AreEqual(expectedBuys, player.PlayerState.Buys);
	}

	private void AssertPile(List<Card> expected, Pile actual)
	{
		Assert.AreSequenceEqual(expected, actual.Select(c => c.Card).ToList(), SequenceOrder.InAnyOrder);
	}

	private void AssertPile(List<Card> expected, List<Card> actual)
	{
		Assert.AreSequenceEqual(expected, actual, SequenceOrder.InAnyOrder);
	}

	private void AssertPile(List<Card> expected, List<CardInstance> actual)
	{
		Assert.AreSequenceEqual(expected, actual.Select(c => c.Card).ToList(), SequenceOrder.InAnyOrder);
	}

	private Pile CreatePile(List<Card> cards)
	{
		return new(cards, kingdom);
	}
}
