using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerBureaucratTests : CardWithPlayerTestsBase
{
	private readonly Card bureaucrat = Bureaucrat.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card province = Province.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(bureaucrat);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void AttackPlayerThatHasVictory()
	{
		#region arrange
		defenderUser.Setup(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom)).Returns(province);

		attacker.PlayerState.Hand = new List<Card> { bureaucrat };
		defender.PlayerState.Hand = new List<Card> { province, duchy };
		#endregion

		#region act
		attacker.PlayActionCardInternal(bureaucrat);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// attacker got silver to his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver }, attacker.PlayerState.DrawPile);

		// bureaucrat was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { bureaucrat }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// defender's user is asked to choose a victory card to put on top
		defenderUser.Verify(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom), Times.Once);

		// province was put on defender's draw pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, defender.PlayerState.DrawPile);

		// the other card stayed in the defender's hand
		CollectionAssert.AreEquivalent(new List<Card> { duchy }, defender.PlayerState.Hand);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasNoVictoryCard()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { bureaucrat };
		#endregion

		#region act
		attacker.PlayActionCardInternal(bureaucrat);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// attacker got silver to his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver }, attacker.PlayerState.DrawPile);

		// bureaucrat was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { bureaucrat }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// defender's user is not asked to choose a victory card to put on top
		defenderUser.Verify(du => du.BureaucratPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// nothing was put on the defender's draw pile
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());

		// nothing was added to the defender's hand
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	[DataRow(0, 0)]
	[DataRow(1, 1)]
	[DataRow(2, 2)]
	[DataRow(3, 2)]
	public void ThroneRoomPlay(int provinceCount, int provincesPutOnTop)
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { throneRoom, bureaucrat };
		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == bureaucrat))).Returns(bureaucrat);

		defender.PlayerState.Hand = Enumerable.Repeat(province, provinceCount).ToList();
		defender.PlayerState.Hand.AddRange(new List<Card> { silver, bureaucrat });
		defenderUser.Setup(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom)).Returns(province);
		#endregion

		#region act
		attacker.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// attacker got silver to his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, attacker.PlayerState.DrawPile);

		// attacker was asked which card to play using throne room
		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// bureaucrat and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { bureaucrat, throneRoom }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// defender's user is asked to choose a victory card to put on top given amount of times
		defenderUser.Verify(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom), Times.Exactly(provincesPutOnTop));

		// if there were up to two provinces in defender's hand, they are now at his draw pile
		CollectionAssert.AreEquivalent(Enumerable.Repeat(province, provincesPutOnTop).ToList(), defender.PlayerState.DrawPile);

		// non-victory cards stayed in defender's hand
		var expectedHand = new List<Card> { silver, bureaucrat };
		expectedHand.AddRange(Enumerable.Repeat(province, provinceCount - provincesPutOnTop));
		CollectionAssert.AreEquivalent(expectedHand, defender.PlayerState.Hand);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}
}