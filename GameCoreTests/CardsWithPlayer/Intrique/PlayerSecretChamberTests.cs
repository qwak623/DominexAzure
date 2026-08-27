using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerSecretChamberTests : CardWithPlayerTestsBase
{
	private readonly Card secretChamber = SecretChamber.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card militia = Militia.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { secretChamber, militia });

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void DiscardsCardsForCoins()
	{
		#region arrange
		defender.PlayerState.Hand = CreatePile([secretChamber, copper, silver]);
		var secretChamberToPlay = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.SecretChamber);
		var cardsToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Name != CardName.SecretChamber).ToList();

		defenderUser.Setup(u => u.SecretChamberDiscard(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(cardsToDiscard);
		#endregion

		#region act
		defender.PlayActionCardInternal(secretChamberToPlay);
		#endregion

		#region assert
		// +$1 per card discarded
		AssertNumbers(0, 2, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, silver], defender.PlayerState.DiscardPile);
		AssertPile([secretChamber], defender.PlayerState.CardsPlayed);
		AssertPile([secretChamber], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.SecretChamberDiscard(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DiscardsNothing()
	{
		#region arrange
		defender.PlayerState.Hand = CreatePile([secretChamber, copper]);
		var secretChamberToPlay = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.SecretChamber);

		defenderUser.Setup(u => u.SecretChamberDiscard(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns([]);
		#endregion

		#region act
		defender.PlayActionCardInternal(secretChamberToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([secretChamber], defender.PlayerState.CardsPlayed);
		AssertPile([secretChamber], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void RevealsToDrawThenPutsTwoBackButAttackStillLands()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([secretChamber, copper, copper, copper]);
		defender.PlayerState.DrawPile = CreatePile([silver, silver]);
		var secretChamberInHand = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.SecretChamber);

		defenderUser.Setup(u => u.PlayCard(It.Is<List<CardInstance>>(r => r.Single() == secretChamberInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns(secretChamberInHand);

		// put the two newly-drawn silvers back on top of the deck
		defenderUser.Setup(u => u.SecretChamberPutOnDeck(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>, int>((c, ps, k, cardSelection, count) => ps.Hand.Where(h => h.Card.Name == CardName.Silver).ToList());

		var copperToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Name == CardName.Copper).Take(1).ToList();
		defenderUser.Setup(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1))
			.Returns(copperToDiscard);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// secret chamber does not defend against the attack - militia still forces a discard
		// down afterward, on top of secret chamber's own draw-then-return
		AssertNumbers(0, 0, 0, defender);
		AssertPile([secretChamber, copper, copper], defender.PlayerState.Hand);
		AssertPile([silver, silver], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.PlayCard(It.IsAny<List<CardInstance>>(),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia), Times.Once);
		defenderUser.Verify(u => u.SecretChamberPutOnDeck(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Once);
		defenderUser.Verify(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DeclinesToRevealAndAttackLandsNormally()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([secretChamber, copper, copper, copper]);
		var secretChamberInHand = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.SecretChamber);
		var copperToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Name == CardName.Copper).Take(1).ToList();

		defenderUser.Setup(u => u.PlayCard(It.Is<List<CardInstance>>(r => r.Single() == secretChamberInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns((CardInstance)null);
		defenderUser.Setup(u => u.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1))
			.Returns(copperToDiscard);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([secretChamber, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.SecretChamberPutOnDeck(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AsksForFewerThanTwoWhenHandIsTooSmall()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		// secret chamber is the defender's only card - drawing 2 does nothing since the deck is empty
		defender.PlayerState.Hand = CreatePile([secretChamber]);
		var secretChamberInHand = defender.PlayerState.Hand[0];

		defenderUser.Setup(u => u.PlayCard(It.Is<List<CardInstance>>(r => r.Single() == secretChamberInHand),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns(secretChamberInHand);
		defenderUser.Setup(u => u.SecretChamberPutOnDeck(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1))
			.Returns([]);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// the defender only has 1 card (itself) after the (empty) draw, so the engine only
		// asks for 1 card to put back, not a hardcoded 2
		defenderUser.Verify(u => u.SecretChamberPutOnDeck(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1), Times.Once);
		defenderUser.Verify(u => u.SecretChamberPutOnDeck(secretChamber, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Never);
		#endregion
	}
}
