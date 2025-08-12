using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerThiefTests : CardWithPlayerTestsBase
{
	private readonly Card thief = Thief.Get();
	private readonly Card province = Province.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(thief);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void Attack_NoCardsToShow()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// attacker did not steal anything
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// defender has nothing to discard
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// nothing was trashed
		Assert.IsFalse(defender.Game.Trash.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_NoTreasuresToSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };

		defender.PlayerState.DrawPile = new List<Card> { province, province };
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender discards the shown cards
		CollectionAssert.AreEquivalent(new List<Card> { province, province }, defender.PlayerState.DiscardPile);

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// the attacker is not asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IEnumerable<Card>>()), Times.Never);

		// the attacker is not asked whether to steal anything
		attackerUser.Verify(au => au.ThiefSteal(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// nothing was trashed
		Assert.IsFalse(defender.Game.Trash.Any());

		// attacker did not gain anything
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_DontSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };

		defender.PlayerState.DrawPile = new List<Card> { copper, province };

		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == copper))).Returns(copper);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			copper)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			copper), Times.Once);

		// the copper is trashed
		CollectionAssert.AreEquivalent(new List<Card> { copper }, defender.Game.Trash.ToList());

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// defender discards the other card
		CollectionAssert.AreEquivalent(new List<Card> { province }, defender.PlayerState.DiscardPile);

		// attacker did not gain anything
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_Steal()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };

		defender.PlayerState.DrawPile = new List<Card> { province, gold };

		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == gold))).Returns(gold);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold), Times.Once);

		// the gold was trashed, but the thief stole it
		Assert.IsFalse(defender.Game.Trash.Any());

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// defender discards the other card
		CollectionAssert.AreEquivalent(new List<Card> { province }, defender.PlayerState.DiscardPile);

		// attacker gained the gold
		CollectionAssert.AreEquivalent(new List<Card> { gold }, attacker.PlayerState.DiscardPile);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresDontSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };

		defender.PlayerState.DrawPile = new List<Card> { copper, silver };

		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.Contains(copper) && c.Contains(silver)))).Returns(silver);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silver)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silver), Times.Once);

		// the silver is trashed
		CollectionAssert.AreEquivalent(new List<Card> { silver }, defender.Game.Trash.ToList());

		// defender discards the other card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, defender.PlayerState.DiscardPile);

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// attacker did not gain anything
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { thief };

		defender.PlayerState.DrawPile = new List<Card> { gold, copper };

		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.Contains(copper) && c.Contains(gold)))).Returns(gold);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(thief);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// a+0 Cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// thief was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold), Times.Once);

		// the gold was trashed, but the thief stole it
		Assert.IsFalse(defender.Game.Trash.Any());

		// defender's hand did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());

		// defender discards the other card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, defender.PlayerState.DiscardPile);

		// attacker gained the gold
		CollectionAssert.AreEquivalent(new List<Card> { gold }, attacker.PlayerState.DiscardPile);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { throneRoom, thief };
		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == thief))).Returns(thief);

		attackerUser.SetupSequence(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>())).Returns(gold).Returns(silver);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold)).Returns(true);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silver)).Returns(false);

		defender.PlayerState.DrawPile = new List<Card> { silver, copper, copper, gold };
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
		Assert.IsFalse(attacker.PlayerState.DrawPile.Any());
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// attacker was asked which card to play using throne room
		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked to choose a treasure to trash two times
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Exactly(2));

		// the attacker is asked whether to steal the trashed card two times
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			gold), Times.Once);
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silver), Times.Once);

		// thief and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { thief, throneRoom }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);
		Assert.IsFalse(defender.PlayerState.Hand.Any());
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());

		// silver and gold were trashed, but the thief stole the gold
		CollectionAssert.AreEquivalent(new List<Card> { silver }, defender.Game.Trash.ToList());

		// defender discards the other cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, defender.PlayerState.DiscardPile);

		// attacker gained the gold
		CollectionAssert.AreEquivalent(new List<Card> { gold }, attacker.PlayerState.DiscardPile);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}
}