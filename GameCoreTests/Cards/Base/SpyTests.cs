using System.Numerics;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class SpyTests : CardTestsBase
{
	private readonly Card spy = Spy.Get();
	private readonly Card province = Province.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(spy);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);

		var players = new List<IPlayer> { attacker.Object, defender.Object };
		attacker.Setup(a => a.Game.Players).Returns(players);
		defender.Setup(a => a.Game.Players).Returns(players);
	}

	[TestMethod]
	public void PlayerDiscardsCard()
	{
		#region arrange
		attacker.Setup(a => a.Show(1)).Returns(new List<Card> { province });
		attacker
			.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, province, Phase.Action))
			.Returns(true);
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +1 Card
		attacker.Verify(p => p.Draw(1), Times.Once);

		// player shows one card
		attacker.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Action), Times.Once);

		// the card is added to the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, attacker.Object.PlayerState.DiscardPile);

		// the card is not added to the draw pile
		Assert.IsFalse(attacker.Object.PlayerState.DrawPile.Any());
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntDiscardCard()
	{
		#region arrange
		attacker.Setup(a => a.Show(1)).Returns(new List<Card> { province });
		attacker
			.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, province, Phase.Action))
			.Returns(false);
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +1 Card
		attacker.Verify(p => p.Draw(1), Times.Once);

		// player shows one card
		attacker.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Action), Times.Once);

		// the card is added to the draw pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, attacker.Object.PlayerState.DrawPile);

		// the card is not added to the discard pile
		Assert.IsFalse(attacker.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void NoCardToDraw()
	{
		#region arrange
		attacker.Setup(a => a.Show(1)).Returns(new List<Card> { });
		attacker.Object.PlayerState.DiscardPile = new List<Card> { };
		attacker.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +1 Card
		attacker.Verify(p => p.Draw(1), Times.Once);

		// player shows one card (it returns an empty list)
		attacker.Verify(a => a.Show(1), Times.Once);

		// user is not asked whether to discard anything
		attacker.Verify(a => a.User.SpyDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Card>(), It.IsAny<Phase>()), Times.Never);

		// nothing is added to the draw pile
		Assert.IsFalse(attacker.Object.PlayerState.DrawPile.Any());

		// nothing is added to the discard pile
		Assert.IsFalse(attacker.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public void ThroneRoomPlay(bool discardSpy, bool discardProvince)
	{
		#region arrange
		attacker.Object.PlayerState.Hand = new List<Card> { spy };
		attacker.Setup(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == spy))).Returns(spy);

		attacker.SetupSequence(a => a.Show(1)).Returns(new List<Card> { province }).Returns(new List<Card> { spy });
		attacker.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, province, Phase.Action))
			.Returns(discardProvince);
		attacker.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, spy, Phase.Action))
			.Returns(discardSpy);
		#endregion

		#region act
		throneRoom.WhenPlayAction(attacker.Object);

		#endregion
		#region assert
		// (+1 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(2, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// (+1 Card) * 2
		attacker.Verify(p => p.Draw(1), Times.Exactly(2));

		// user is asked which card to play using throne room
		attacker.Verify(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player shows one card two times
		attacker.Verify(a => a.Show(1), Times.Exactly(2));

		// user is asked whether to discard province and spy
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Action), Times.Once);
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			spy, Phase.Action), Times.Once);

		// the spy is added to the discard or draw pile 
		if (discardSpy)
		{
			CollectionAssert.Contains(attacker.Object.PlayerState.DiscardPile, spy);
			CollectionAssert.DoesNotContain(attacker.Object.PlayerState.DrawPile, spy);
		}
		else
		{
			CollectionAssert.Contains(attacker.Object.PlayerState.DrawPile, spy);
			CollectionAssert.DoesNotContain(attacker.Object.PlayerState.DiscardPile, spy);
		}

		// the province is added to the discard or draw pile 
		if (discardProvince)
		{
			CollectionAssert.Contains(attacker.Object.PlayerState.DiscardPile, province);
			CollectionAssert.DoesNotContain(attacker.Object.PlayerState.DrawPile, province);
		}
		else
		{
			CollectionAssert.Contains(attacker.Object.PlayerState.DrawPile, province);
			CollectionAssert.DoesNotContain(attacker.Object.PlayerState.DiscardPile, province);
		}

		// attacker deals an attack to the defender two times
		defender.Verify(d => d.DealAttack(attacker.Object, spy), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void Attack_PlayerDiscardsCard()
	{
		#region arrange
		defender.Setup(d => d.Show(1)).Returns(new List<Card> { province });
		attacker
			.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, province, Phase.Attack))
			.Returns(true);
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		defender.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender shows one card
		defender.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Attack), Times.Once);

		// the card is added to the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, defender.Object.PlayerState.DiscardPile);

		// the card is not added to the draw pile
		Assert.IsFalse(defender.Object.PlayerState.DrawPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_PlayerDoesntDiscardCard()
	{
		#region arrange
		defender.Setup(a => a.Show(1)).Returns(new List<Card> { province });
		attacker
			.Setup(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom, province, Phase.Attack))
			.Returns(false);
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		defender.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// player shows one card
		defender.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Attack), Times.Once);

		// the card is added to the draw pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, defender.Object.PlayerState.DrawPile);

		// the card is not added to the discard pile
		Assert.IsFalse(defender.Object.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void Attack_NoCardToDraw()
	{
		#region arrange
		defender.Setup(d => d.Show(1)).Returns(new List<Card> { });
		defender.Object.PlayerState.DiscardPile = new List<Card> { };
		defender.Object.PlayerState.DrawPile = new List<Card> { };
		#endregion

		#region act
		spy.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// player shows one card (it returns an empty list)
		defender.Verify(a => a.Show(1), Times.Once);

		// user is not asked whether to discard anything
		attacker.Verify(a => a.User.SpyDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<Card>(), It.IsAny<Phase>()), Times.Never);

		// nothing is added to the draw pile
		Assert.IsFalse(defender.Object.PlayerState.DrawPile.Any());

		// nothing is added to the discard pile
		Assert.IsFalse(defender.Object.PlayerState.DiscardPile.Any());
		#endregion
	}
}