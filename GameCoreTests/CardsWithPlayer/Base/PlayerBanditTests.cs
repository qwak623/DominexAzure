using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerBanditTests : CardWithPlayerTestsBase
{
	private readonly Card bandit = Bandit.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(bandit);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void GainsAGoldRegardlessOfTheAttack()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bandit]);
		var banditToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.DrawPile = CreatePile([]);
		defender.PlayerState.DiscardPile = CreatePile([]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(banditToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([gold], attacker.PlayerState.DiscardPile);
		AssertPile([bandit], attacker.PlayerState.CardsPlayed);
		AssertPile([bandit], attacker.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void AutoTrashesTheOnlyNonCopperTreasureRevealed()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bandit]);
		var banditToPlay = attacker.PlayerState.Hand[0];

		// bottom to top: gold is revealed first (top), copper second
		defender.PlayerState.DrawPile = CreatePile([copper, gold]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(banditToPlay);
		#endregion

		#region assert
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([gold], defender.Game.Trash);
		AssertPile([copper], defender.PlayerState.DiscardPile);

		// only one non-copper treasure was revealed, so there is nothing to choose between
		attackerUser.Verify(u => u.BanditTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AsksToChooseWhenTwoNonCopperTreasuresAreRevealed()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bandit]);
		var banditToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.DrawPile = CreatePile([silver, gold]);

		attackerUser.Setup(u => u.BanditTrash(bandit, defender.PlayerState, defender.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2))).Returns<Card, PlayerState, Kingdom, List<CardInstance>>(
			(c, ps, k, cards) => cards.Single(x => x.Card.Name == CardName.Silver));
		#endregion

		#region act
		attacker.PlayActionCardInternal(banditToPlay);
		#endregion

		#region assert
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([silver], defender.Game.Trash);
		AssertPile([gold], defender.PlayerState.DiscardPile);

		attackerUser.Verify(u => u.BanditTrash(bandit, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DiscardsBothRevealedCardsWhenNeitherIsATrashableTreasure()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bandit]);
		var banditToPlay = attacker.PlayerState.Hand[0];

		// copper doesn't count, so there's nothing to trash here
		defender.PlayerState.DrawPile = CreatePile([copper, copper]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(banditToPlay);
		#endregion

		#region assert
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.Game.Trash);
		AssertPile([copper, copper], defender.PlayerState.DiscardPile);

		attackerUser.Verify(u => u.BanditTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void NothingHappensWhenDefenderHasNoCardsToReveal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bandit]);
		var banditToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.DrawPile = CreatePile([]);
		defender.PlayerState.DiscardPile = CreatePile([]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(banditToPlay);
		#endregion

		#region assert
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.BanditTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
