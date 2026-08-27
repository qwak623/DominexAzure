using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerCourtierTests : CardWithPlayerTestsBase
{
	private readonly Card courtier = Courtier.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card curse = Curse.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card witch = Witch.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(courtier);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void RevealingASingleTypeCardOffersOneBenefitChoice()
	{
		#region arrange
		// copper is a treasure and nothing else, so there's exactly one type to choose a benefit for
		player.PlayerState.Hand = CreatePile([courtier, copper]);
		var courtierToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Courtier);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);

		user.Setup(u => u.CourtierReveal(courtier, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);
		user.Setup(u => u.CourtierChooseBenefits(courtier, player.PlayerState, player.Game.Kingdom, 1, It.IsAny<List<CourtierBenefit>>()))
			.Returns([CourtierBenefit.GainGold]);
		#endregion

		#region act
		player.PlayActionCardInternal(courtierToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([gold], player.PlayerState.DiscardPile);
		AssertPile([courtier], player.PlayerState.CardsPlayed);
		AssertPile([courtier], player.PlayerState.ActionsPlayed);

		// the client is offered all four benefits to choose exactly one from
		user.Verify(u => u.CourtierChooseBenefits(courtier, player.PlayerState, player.Game.Kingdom, 1,
			It.Is<List<CourtierBenefit>>(b => b.Count == 4 && b.Contains(CourtierBenefit.Action) && b.Contains(CourtierBenefit.Buy)
				&& b.Contains(CourtierBenefit.Coins) && b.Contains(CourtierBenefit.GainGold))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void RevealingATwoTypeCardOffersTwoBenefitChoicesAndAppliesBoth()
	{
		#region arrange
		// witch is both an Action and an Attack, so two distinct benefits must be chosen
		player.PlayerState.Hand = CreatePile([courtier, witch]);
		var courtierToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Courtier);
		var witchInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Witch);

		user.Setup(u => u.CourtierReveal(courtier, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(witchInHand);
		user.Setup(u => u.CourtierChooseBenefits(courtier, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<CourtierBenefit>>()))
			.Returns([CourtierBenefit.Action, CourtierBenefit.Coins]);
		#endregion

		#region act
		player.PlayActionCardInternal(courtierToPlay);
		#endregion

		#region assert
		// +1 action from the chosen benefit cancels out the action spent playing courtier itself
		AssertNumbers(1, 3, 0, player);
		AssertPile([witch], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([courtier], player.PlayerState.CardsPlayed);
		AssertPile([courtier], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.CourtierChooseBenefits(courtier, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<CourtierBenefit>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void RevealingAVictoryOnlyCardOffersOneBenefitChoice()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([courtier, duchy]);
		var courtierToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Courtier);
		var duchyInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Duchy);

		user.Setup(u => u.CourtierReveal(courtier, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(duchyInHand);
		user.Setup(u => u.CourtierChooseBenefits(courtier, player.PlayerState, player.Game.Kingdom, 1, It.IsAny<List<CourtierBenefit>>()))
			.Returns([CourtierBenefit.Coins]);
		#endregion

		#region act
		player.PlayActionCardInternal(courtierToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 3, 0, player);
		AssertPile([duchy], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void NoCardsInHandToReveal()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([courtier]);
		var courtierToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(courtierToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([courtier], player.PlayerState.CardsPlayed);
		AssertPile([courtier], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.CourtierReveal(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.CourtierChooseBenefits(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>(), It.IsAny<List<CourtierBenefit>>()), Times.Never);
		#endregion
	}
}
