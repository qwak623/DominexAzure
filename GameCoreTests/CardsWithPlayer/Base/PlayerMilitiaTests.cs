using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMilitiaTests : CardWithPlayerTestsBase
{
	private readonly Card militia = Militia.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(militia);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod()]
	public void Attack_DiscardTwoCards()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { militia };

		defender.PlayerState.Hand = new List<Card> { silver, silver, silver, silver, copper };
		defenderUser.Setup(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 2))
			.Returns(new List<Card> { silver, copper });
		#endregion

		#region act
		attacker.PlayActionCardInternal(militia);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, attacker.PlayerState.Coins);

		// actions and buys shouldn't change
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// attacker does not draw any cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// militia was added to the attacker's played cards
		CollectionAssert.AreEqual(new List<Card> { militia }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender's user is asked to choose two cards to discard
		defenderUser.Verify(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 2), Times.Once);

		// defender discards two cards
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver, silver }, defender.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper }, defender.PlayerState.DiscardPile);

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod()]
	public void Attack_DontDiscardAnything()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { militia };

		defender.PlayerState.Hand = new List<Card> { copper, copper, silver };
		#endregion

		#region act
		attacker.PlayActionCardInternal(militia);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, attacker.PlayerState.Coins);

		// actions and buys shouldn't change
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// attacker does not draw any cards
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// militia was added to the attacker's played cards
		CollectionAssert.AreEqual(new List<Card> { militia }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender's user is never asked to choose cards to discard
		defenderUser.Verify(du => du.MilitiaDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);

		// defender doesn't discard anything
		CollectionAssert.AreEquivalent(new List<Card> { silver, copper, copper }, defender.PlayerState.Hand);
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}
}