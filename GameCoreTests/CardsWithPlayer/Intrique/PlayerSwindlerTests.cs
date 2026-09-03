using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerSwindlerTests : CardWithPlayerTestsBase
{
	private readonly Card swindler = Swindler.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card bureaucrat = Bureaucrat.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(swindler);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void TrashesTopCardAndGainsTheChosenReplacement()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([swindler]);
		var swindlerToPlay = attacker.PlayerState.Hand[0];

		// bottom to top: copper stays, silver ($3) is on top and gets trashed
		defender.PlayerState.DrawPile = CreatePile([copper, silver]);

		var gainedSilver = game.Object.Kingdom.GetPile(CardName.Silver).CardInstance;
		attackerUser.Setup(u => u.SelectCardToGain(swindler, defender.PlayerState, defender.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(defender.PlayerState) == 3)))).Returns(gainedSilver);
		#endregion

		#region act
		attacker.PlayActionCardInternal(swindlerToPlay);
		#endregion

		#region assert
		// +$2 from swindler itself, no card draw
		AssertNumbers(0, 2, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([swindler], attacker.PlayerState.CardsPlayed);
		AssertPile([swindler], attacker.PlayerState.ActionsPlayed);

		// the trashed top card is gone from the draw pile and the replacement lands in discard
		AssertPile([copper], defender.PlayerState.DrawPile);
		AssertPile([silver], defender.Game.Trash);
		AssertPile([silver], defender.PlayerState.DiscardPile);

		// the attacker chooses the replacement, not the defender
		attackerUser.Verify(u => u.SelectCardToGain(swindler, defender.PlayerState, defender.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(defender.PlayerState) == 3))), Times.Once);
		defenderUser.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void NothingHappensWhenDefenderHasNoCardsToTrash()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([swindler]);
		var swindlerToPlay = attacker.PlayerState.Hand[0];

		// both draw and discard pile empty, so Show(1) has nothing to reveal
		defender.PlayerState.DrawPile = CreatePile([]);
		defender.PlayerState.DiscardPile = CreatePile([]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(swindlerToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, attacker);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void NothingIsGainedWhenNoCardOfTheSameCostIsAvailable()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([swindler]);
		var swindlerToPlay = attacker.PlayerState.Hand[0];

		// bureaucrat costs $4, and nothing in this kingdom (just swindler, plus the basic
		// treasures/victories always added) costs $4, so there is genuinely nothing to gain -
		// the attacker isn't just choosing to decline a real option
		defender.PlayerState.DrawPile = CreatePile([copper, bureaucrat]);

		attackerUser.Setup(u => u.SelectCardToGain(swindler, defender.PlayerState, defender.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 0))).Returns((CardInstance)null);
		#endregion

		#region act
		attacker.PlayActionCardInternal(swindlerToPlay);
		#endregion

		#region assert
		// the top card is still trashed even though no replacement was gained
		AssertPile([copper], defender.PlayerState.DrawPile);
		AssertPile([bureaucrat], defender.Game.Trash);
		AssertPile([], defender.PlayerState.DiscardPile);
		#endregion
	}
}
