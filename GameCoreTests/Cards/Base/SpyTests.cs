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

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(spy);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);
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
		// +1 Action
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player draws one card
		attacker.Verify(p => p.Draw(1), Times.Once);

		// player shows one card
		attacker.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Action), Times.Once);

		// the card is added to the discard pile
		CollectionAssert.AreEqual(new List<Card> { province }, attacker.Object.PlayerState.DiscardPile);

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
		// +1 Action
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player draws one card
		attacker.Verify(p => p.Draw(1), Times.Once);

		// player shows one card
		attacker.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Action), Times.Once);

		// the card is added to the draw pile
		CollectionAssert.AreEqual(new List<Card> { province }, attacker.Object.PlayerState.DrawPile);

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
		// +1 Action
		Assert.AreEqual(1, attacker.Object.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player draws one card
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
		// player shows one card
		defender.Verify(a => a.Show(1), Times.Once);

		// user is asked whether to discard the card
		attacker.Verify(a => a.User.SpyDiscard(spy, attacker.Object.PlayerState, attacker.Object.Game.Kingdom,
			province, Phase.Attack), Times.Once);

		// the card is added to the discard pile
		CollectionAssert.AreEqual(new List<Card> { province }, defender.Object.PlayerState.DiscardPile);

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
		CollectionAssert.AreEqual(new List<Card> { province }, defender.Object.PlayerState.DrawPile);

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