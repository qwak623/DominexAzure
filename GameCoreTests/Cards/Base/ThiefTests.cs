using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class ThiefTests : CardTestsBase
{
	private readonly Card thief = Thief.Get();
	private readonly Card province = Province.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(thief);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);

		// mock trash
		var trash = new List<Card> { };
		attacker.Setup(d => d.Game.Trash).Returns(trash);
		defender.Setup(d => d.Game.Trash).Returns(trash);
	}

	[TestMethod]
	public void Play()
	{
		#region act
		thief.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player does not draw a card
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void Attack_NoCardsToShow()
	{
		#region arrange
		defender.Setup(d => d.Show(2)).Returns(new List<Card> { });
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// defender has nothing to discard
		Assert.IsFalse(defender.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_NoTreasuresToSteal()
	{
		#region arrange
		defender.Setup(d => d.Show(2)).Returns(new List<Card> { province, province });
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// defender discards the shown cards
		CollectionAssert.AreEqual(new List<Card> { province, province }, defender.Object.PlayerState.DiscardPile);

		// the attacker is not asked to choose a treasure to trash
		attacker.Verify(a => a.User.ThiefChoose(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IEnumerable<Card>>()), Times.Never);

		// the attacker is not asked whether to steal anything
		attacker.Verify(a => a.User.ThiefSteal(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Card>()), Times.Never);

		// nothing was trashed
		Assert.IsFalse(defender.Object.Game.Trash.Any());

		// attacker did not gain anything
		Assert.IsFalse(attacker.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_DontSteal()
	{
		#region arrange
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };

		defender.Setup(d => d.Show(2)).Returns(new List<Card> { copper, province });
		attacker.Setup(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == copper))).Returns(copper);
		attacker.Setup(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			copper)).Returns(false);
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// the attacker is asked to choose a treasure to trash
		attacker.Verify(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attacker.Verify(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			copper), Times.Once);

		// the copper is trashed
		CollectionAssert.AreEqual(new List<Card> { copper }, defender.Object.Game.Trash.ToList());

		// defender discards the other card
		CollectionAssert.AreEqual(new List<Card> { province }, defender.Object.PlayerState.DiscardPile);

		// attacker did not gain anything
		Assert.IsFalse(attacker.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_Steal()
	{
		#region arrange
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };

		defender.Setup(d => d.Show(2)).Returns(new List<Card> { province, gold });
		attacker.Setup(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == gold))).Returns(gold);
		attacker.Setup(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			gold)).Returns(true);
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// the attacker is asked to choose a treasure to trash
		attacker.Verify(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attacker.Verify(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			gold), Times.Once);

		// the gold was trashed, but the thief stole it
		Assert.IsFalse(defender.Object.Game.Trash.Any());

		// defender discards the other card
		CollectionAssert.AreEqual(new List<Card> { province }, defender.Object.PlayerState.DiscardPile);

		// attacker gained the gold
		CollectionAssert.AreEqual(new List<Card> { gold }, attacker.Object.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresDontSteal()
	{
		#region arrange
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };

		defender.Setup(d => d.Show(2)).Returns(new List<Card> { copper, silver });
		attacker.Setup(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.Contains(copper) && c.Contains(silver)))).Returns(silver);
		attacker.Setup(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			silver)).Returns(false);
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// the attacker is asked to choose a treasure to trash
		attacker.Verify(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attacker.Verify(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			silver), Times.Once);

		// the silver is trashed
		CollectionAssert.AreEqual(new List<Card> { silver }, defender.Object.Game.Trash.ToList());

		// defender discards the other card
		CollectionAssert.AreEqual(new List<Card> { copper }, defender.Object.PlayerState.DiscardPile);

		// attacker did not gain anything
		Assert.IsFalse(attacker.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresSteal()
	{
		#region arrange
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };

		defender.Setup(d => d.Show(2)).Returns(new List<Card> { gold, copper });
		attacker.Setup(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.Is<IEnumerable<Card>>(c => c.Count() == 2 && c.Contains(copper) && c.Contains(gold)))).Returns(gold);
		attacker.Setup(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			gold)).Returns(true);
		#endregion

		#region act
		thief.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows two cards
		defender.Verify(d => d.Show(2), Times.Once);

		// the attacker is asked to choose a treasure to trash
		attacker.Verify(a => a.User.ThiefChoose(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			It.IsAny<IEnumerable<Card>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attacker.Verify(a => a.User.ThiefSteal(thief, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			gold), Times.Once);

		// the gold was trashed, but the thief stole it
		Assert.IsFalse(defender.Object.Game.Trash.Any());

		// defender discards the other card
		CollectionAssert.AreEqual(new List<Card> { copper }, defender.Object.PlayerState.DiscardPile);

		// attacker gained the gold
		CollectionAssert.AreEqual(new List<Card> { gold }, attacker.Object.PlayerState.DiscardPile);
		#endregion
	}
}