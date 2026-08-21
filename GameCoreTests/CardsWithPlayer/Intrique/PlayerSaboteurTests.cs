using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerSaboteurTests : CardWithPlayerTestsBase
{
	private readonly Card saboteur = Saboteur.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card estate = Estate.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(saboteur);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void TrashesFirstRevealedCardWhenItCostsThreeOrMore()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([saboteur]);
		var saboteurToPlay = attacker.PlayerState.Hand[0];

		// silver is on top, so it's the very first card revealed
		defender.PlayerState.DrawPile = CreatePile([copper, silver]);

		KingdomWrapper wrapper = null;
		defenderUser.Setup(u => u.SelectOptionalCardToGain(It.IsAny<KingdomWrapper>(), defender.PlayerState, defender.Game.Kingdom, Phase.Attack))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Copper));
		#endregion

		#region act
		attacker.PlayActionCardInternal(saboteurToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([saboteur], attacker.PlayerState.CardsPlayed);
		AssertPile([saboteur], attacker.PlayerState.ActionsPlayed);
		AssertPile([silver], attacker.Game.Trash);

		// nothing was revealed-and-discarded first, so only the gained copper ends up here
		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);

		// silver costs 3, so the replacement is capped at 3-2=1
		Assert.AreEqual(1, wrapper.MaxPrice);
		#endregion
	}

	[TestMethod]
	public void DiscardsCheaperCardsBeforeFindingOneToTrashAndDeclinesToGain()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([saboteur]);
		var saboteurToPlay = attacker.PlayerState.Hand[0];

		// estate ($2) and copper ($0) get revealed and discarded first, silver ($3) stops the search
		defender.PlayerState.DrawPile = CreatePile([silver, copper, estate]);

		defenderUser.Setup(u => u.SelectOptionalCardToGain(It.IsAny<KingdomWrapper>(), defender.PlayerState, defender.Game.Kingdom, Phase.Attack))
			.Returns((CardInstance)null);
		#endregion

		#region act
		attacker.PlayActionCardInternal(saboteurToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([saboteur], attacker.PlayerState.CardsPlayed);
		AssertPile([saboteur], attacker.PlayerState.ActionsPlayed);
		AssertPile([silver], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([estate, copper], defender.PlayerState.DiscardPile);

		defenderUser.Verify(u => u.SelectOptionalCardToGain(It.IsAny<KingdomWrapper>(), defender.PlayerState, defender.Game.Kingdom, Phase.Attack), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GainsReplacementCostingUpToTwoLessThanTrashedCard()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([saboteur]);
		var saboteurToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.DrawPile = CreatePile([gold]);

		// selection is pulled from the wrapper itself, so this only succeeds if silver
		// genuinely passes the wrapper's own availability check at the computed cap
		KingdomWrapper wrapper = null;
		defenderUser.Setup(u => u.SelectOptionalCardToGain(It.IsAny<KingdomWrapper>(), defender.PlayerState, defender.Game.Kingdom, Phase.Attack))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Silver));
		#endregion

		#region act
		attacker.PlayActionCardInternal(saboteurToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([silver], defender.PlayerState.DiscardPile);
		AssertPile([gold], attacker.Game.Trash);

		// gold costs 6, so the replacement is capped at 6-2=4
		Assert.AreEqual(4, wrapper.MaxPrice);
		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Silver));
		Assert.IsFalse(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Gold));
		#endregion
	}

	[TestMethod]
	public void DiscardsEntireDeckWhenNoCardCostsThreeOrMore()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([saboteur]);
		var saboteurToPlay = attacker.PlayerState.Hand[0];

		// nothing here costs 3 or more, and there's nothing left to reshuffle from
		defender.PlayerState.DrawPile = CreatePile([copper, estate]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(saboteurToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([saboteur], attacker.PlayerState.CardsPlayed);
		AssertPile([saboteur], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([estate, copper], defender.PlayerState.DiscardPile);

		defenderUser.Verify(u => u.SelectOptionalCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);
		#endregion
	}
}
