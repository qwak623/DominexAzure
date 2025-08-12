using System.Numerics;
using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerWitchTests : CardWithPlayerTestsBase
{
	private readonly Card witch = Witch.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card curse = Curse.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(witch);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { witch };
		attacker.PlayerState.DrawPile = new List<Card> { silver, silver };
		#endregion

		#region act
		attacker.PlayActionCardInternal(witch);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, attacker.PlayerState.Actions);

		// +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// +2 Cards
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, attacker.PlayerState.Hand);
		Assert.IsFalse(attacker.PlayerState.DrawPile.Any());

		// attacker's discard pile did not change
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// witch was added to the attacker's played cards
		CollectionAssert.AreEquivalent(new List<Card> { witch }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender got a curse to his discard pile
		CollectionAssert.AreEquivalent(new List<Card> { curse }, defender.PlayerState.DiscardPile);

		// defender's hand and draw pile did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.PlayerState.Hand = new List<Card> { throneRoom, witch };
		attacker.PlayerState.DrawPile = new List<Card> { silver, silver, curse, silver };

		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == witch))).Returns(witch);

		#endregion
		#region act
		attacker.PlayActionCardInternal(throneRoom);

		#endregion
		#region assert
		// -1 Action, (+0 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, attacker.PlayerState.Actions);
		Assert.AreEqual(0, attacker.PlayerState.Coins);
		Assert.AreEqual(0, attacker.PlayerState.Buys);

		// (+2 Card) * 2
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver, curse, silver }, attacker.PlayerState.Hand);
		Assert.IsFalse(attacker.PlayerState.DrawPile.Any());
		Assert.IsFalse(attacker.PlayerState.DiscardPile.Any());

		// attacker was asked which card to play using throne room
		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// witch and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { witch, throneRoom }, attacker.PlayerState.PlayedCards);

		// defender's actions, coins and buys shouldn't change
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// defender got two curses to his discard pile
		CollectionAssert.AreEquivalent(new List<Card> { curse, curse }, defender.PlayerState.DiscardPile);

		// defender's hand and draw pile did not change
		Assert.IsFalse(defender.PlayerState.Hand.Any());
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());

		// nothing was added to the defender's played cards
		Assert.IsFalse(defender.PlayerState.PlayedCards.Any());
		#endregion
	}
}