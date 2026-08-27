using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerMinionTests : CardWithPlayerTestsBase
{
	private readonly Card minion = Minion.Get();
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
		game = MockGame(minion);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void ChoosesCoins()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([minion, copper]);
		var minionToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Minion);

		defender.PlayerState.Hand = CreatePile([silver, silver, silver, silver, silver]);

		attackerUser.Setup(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(minionToPlay);
		#endregion

		#region assert
		// +1 action (minion's own) and +2 coins from the chosen branch; hand untouched, no attack
		AssertNumbers(1, 2, 0, attacker);
		AssertPile([copper], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([minion], attacker.PlayerState.CardsPlayed);
		AssertPile([minion], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, silver, silver, silver, silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DiscardsAndAttacksDefenderWithFiveOrMoreCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([minion, copper]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver, silver]);
		var minionToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Minion);

		defender.PlayerState.Hand = CreatePile([silver, silver, silver, silver, silver]);
		defender.PlayerState.DrawPile = CreatePile([copper, copper, copper, copper]);

		attackerUser.Setup(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(minionToPlay);
		#endregion

		#region assert
		// attacker discards their hand (just the leftover copper - minion itself already moved
		// to cards played) and draws 4; defender has >=5 cards, so they discard and draw 4 too
		AssertNumbers(1, 0, 0, attacker);
		AssertPile([silver, silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([copper], attacker.PlayerState.DiscardPile);
		AssertPile([minion], attacker.PlayerState.CardsPlayed);
		AssertPile([minion], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper, copper, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([silver, silver, silver, silver, silver], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesNotAttackDefenderWithFewerThanFiveCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([minion, copper]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver, silver]);
		var minionToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Minion);

		defender.PlayerState.Hand = CreatePile([silver, silver, silver, silver]);

		attackerUser.Setup(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(minionToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, attacker);
		AssertPile([silver, silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([copper], attacker.PlayerState.DiscardPile);
		AssertPile([minion], attacker.PlayerState.CardsPlayed);
		AssertPile([minion], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// defender has fewer than 5 cards, so minion's attack is a no-op for them
		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, silver, silver, silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomAttacksOnlyOnTheDiscardResolution()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, minion]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver, silver]);
		var minionToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Minion);

		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == minionToPlay))).Returns(minionToPlay);

		// each resolution's branch is chosen independently - the first resolution discards
		// (and attacks), the second just takes the coins
		attackerUser.SetupSequence(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom))
			.Returns(true).Returns(false);

		defender.PlayerState.Hand = CreatePile([copper, copper, copper, copper, copper]);
		defender.PlayerState.DrawPile = CreatePile([gold, gold, gold, gold]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		// +2 actions total (minion's own +1 fires on both resolutions), +2 coins from the
		// second (coins) resolution only; the first (discard) resolution empties and refills
		// the attacker's hand
		AssertNumbers(2, 2, 0, attacker);
		AssertPile([silver, silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([throneRoom, minion], attacker.PlayerState.CardsPlayed);
		AssertPile([throneRoom, minion, minion], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// defender is attacked exactly once, from the first resolution's discard branch -
		// not twice, and not at all from the second (coins) resolution
		AssertNumbers(0, 0, 0, defender);
		AssertPile([gold, gold, gold, gold], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper, copper, copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		attackerUser.Verify(u => u.MinionDiscard(minion, attacker.PlayerState, attacker.Game.Kingdom), Times.Exactly(2));
		#endregion
	}
}
