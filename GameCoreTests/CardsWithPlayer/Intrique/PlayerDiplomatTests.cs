using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerDiplomatTests : CardWithPlayerTestsBase
{
	private readonly Card diplomat = Diplomat.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card militia = Militia.Get();

	private Player player;
	private Player attacker;
	private Player defender;

	private Mock<IUser> user;
	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { diplomat, militia });

		user = new Mock<IUser>();
		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void GrantsTwoActionsWhenHandIsFiveOrFewer()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([diplomat, copper, copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var diplomatToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Diplomat);
		#endregion

		#region act
		player.PlayActionCardInternal(diplomatToPlay);
		#endregion

		#region assert
		// hand after the base +2 Cards draw: 2 coppers + 2 silvers = 4 cards, <=5, so +2 actions fire
		AssertNumbers(2, 0, 0, player);
		AssertPile([copper, copper, silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([diplomat], player.PlayerState.CardsPlayed);
		AssertPile([diplomat], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void DoesNotGrantActionsWhenHandExceedsFive()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([diplomat, copper, copper, copper, copper, copper]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var diplomatToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Diplomat);
		#endregion

		#region act
		player.PlayActionCardInternal(diplomatToPlay);
		#endregion

		#region assert
		// hand after drawing: 5 coppers + 2 silvers = 7 cards, >5, so no bonus actions
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, copper, copper, copper, silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([diplomat], player.PlayerState.CardsPlayed);
		AssertPile([diplomat], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void RevealsToDrawThenDiscardsThreeButAttackStillLands()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([diplomat, copper, copper, copper, copper]);
		defender.PlayerState.DrawPile = CreatePile([silver, silver]);
		var diplomatInHand = defender.PlayerState.Hand.First(c => c.Card.Type == CardType.Diplomat);

		defenderUser.Setup(u => u.PlayCard(It.Is<IEnumerable<CardInstance>>(r => r.Single() == diplomatInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns(diplomatInHand);

		// discard three of the four coppers, keeping diplomat, one copper and the two newly-drawn silvers
		defenderUser.Setup(u => u.DiplomatDiscard(diplomat, defender.PlayerState, defender.Game.Kingdom, 3))
			.Returns<Card, PlayerState, Kingdom, int>((c, ps, k, count) =>
				ps.Hand.Where(h => h.Card.Type == CardType.Copper).Take(3).ToList());

		// diplomat does not block the attack - militia still forces a discard down afterward
		defenderUser.Setup(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 1))
			.Returns<Card, PlayerState, Kingdom, int>((c, ps, k, count) =>
				ps.Hand.Where(h => h.Card.Type == CardType.Copper).ToList());
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([diplomat, silver, silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper, copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.PlayCard(It.IsAny<IEnumerable<CardInstance>>(),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia), Times.Once);
		defenderUser.Verify(u => u.DiplomatDiscard(diplomat, defender.PlayerState, defender.Game.Kingdom, 3), Times.Once);
		defenderUser.Verify(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void CannotUsefullyRevealWithFewerThanFiveCards()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([diplomat, copper, copper, copper]);
		var diplomatInHand = defender.PlayerState.Hand.First(c => c.Card.Type == CardType.Diplomat);
		var copperToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).Take(1).ToList();

		defenderUser.Setup(u => u.PlayCard(It.Is<IEnumerable<CardInstance>>(r => r.Single() == diplomatInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns(diplomatInHand);
		defenderUser.Setup(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 1))
			.Returns(copperToDiscard);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// hand has fewer than 5 cards, so revealing diplomat does nothing - no draw, no discard
		// from diplomat itself, and militia's attack still applies normally
		AssertNumbers(0, 0, 0, defender);
		AssertPile([diplomat, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.DiplomatDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DeclinesToRevealAndAttackLandsNormally()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([diplomat, copper, copper, copper, copper]);
		var diplomatInHand = defender.PlayerState.Hand.First(c => c.Card.Type == CardType.Diplomat);
		var coppersToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).Take(2).ToList();

		defenderUser.Setup(u => u.PlayCard(It.Is<IEnumerable<CardInstance>>(r => r.Single() == diplomatInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns((CardInstance)null);
		defenderUser.Setup(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 2))
			.Returns(coppersToDiscard);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([diplomat, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.DiplomatDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);
		#endregion
	}
}
