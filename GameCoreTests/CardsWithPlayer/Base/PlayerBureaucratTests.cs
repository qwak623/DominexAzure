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
		// -1 Action
		Assert.AreEqual(0, attacker.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// attacker got silver to his draw pile
		CollectionAssert.AreEqual(new List<Card> { silver }, attacker.PlayerState.DrawPile);

		// attacker's hand is now empty
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// bureaucrat was added to the attacker's played cards
		CollectionAssert.AreEqual(new List<Card> { bureaucrat }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender's user is asked to choose a victory card to put on top
		defenderUser.Verify(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom), Times.Once);

		// province was put on defender's draw pile
		CollectionAssert.AreEqual(new List<Card> { province }, defender.PlayerState.DrawPile);

		// the other card stayed in the defender's hand
		CollectionAssert.AreEqual(new List<Card> { duchy }, defender.PlayerState.Hand);

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
		// -1 Action
		Assert.AreEqual(0, attacker.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// attacker got silver to his draw pile
		CollectionAssert.AreEqual(new List<Card> { silver }, attacker.PlayerState.DrawPile);

		// attacker's hand is now empty
		Assert.IsFalse(attacker.PlayerState.Hand.Any());

		// bureaucrat was added to the attacker's played cards
		CollectionAssert.AreEqual(new List<Card> { bureaucrat }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

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
}