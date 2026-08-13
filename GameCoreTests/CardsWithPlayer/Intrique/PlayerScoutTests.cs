using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerScoutTests : CardWithPlayerTestsBase
{
	private readonly Card scout = Scout.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card duchy = Duchy.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(scout);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PutsVictoryCardsInHandAndOrdersTheRest()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([scout]);
		player.PlayerState.DrawPile = CreatePile([copper, estate, silver, duchy]);
		var scoutToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(scoutToPlay);
		#endregion

		#region assert
		// +1 action (scout's own); duchy and estate (victory) go to hand, silver and copper
		// (the two non-victory cards revealed) go back on top of the deck
		AssertNumbers(1, 0, 0, player);
		AssertPile([duchy, estate], player.PlayerState.Hand);
		AssertPile([silver, copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([scout], player.PlayerState.CardsPlayed);
		AssertPile([scout], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// only the two non-victory cards are left to order once the victory cards are pulled out
		user.Verify(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2 && c.All(x => !x.IsVictory))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NoVictoryCardsAreLeftUntouched()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([scout]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, copper, silver]);
		var scoutToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(scoutToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([copper, silver, copper, silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([scout], player.PlayerState.CardsPlayed);
		AssertPile([scout], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 4)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AllRevealedCardsAreVictoryCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([scout]);
		player.PlayerState.DrawPile = CreatePile([estate, estate, duchy, duchy]);
		var scoutToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(scoutToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([estate, estate, duchy, duchy], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([scout], player.PlayerState.CardsPlayed);
		AssertPile([scout], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// nothing left to order once every revealed card was a victory card
		user.Verify(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 0)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void RevealsFewerThanFourWhenDeckIsSmall()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([scout]);
		player.PlayerState.DrawPile = CreatePile([copper, estate]);
		var scoutToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(scoutToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([estate], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([scout], player.PlayerState.CardsPlayed);
		AssertPile([scout], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 1)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AppliesTheChosenOrderToTheTopOfTheDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([scout]);
		player.PlayerState.DrawPile = CreatePile([copper, silver]);
		var scoutToPlay = player.PlayerState.Hand[0];

		// put silver back first (bottom) and copper back last (top), regardless of the order
		// they were originally revealed in
		user.Setup(u => u.ScoutOrderCards(scout, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
				[cards.Single(x => x.Card.Type == CardType.Silver), cards.Single(x => x.Card.Type == CardType.Copper)]);
		#endregion

		#region act
		player.PlayActionCardInternal(scoutToPlay);
		player.Draw(1);
		#endregion

		#region assert
		// copper was placed last in the chosen order, i.e. on top of the deck, so it's the
		// next card drawn
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	[Ignore("TODO: once gain hooks exist (see Player.GainToHand's 'TODO hook on gain event'), add a " +
		"test proving scout's revealed victory cards moving to hand do NOT trigger them - this reuses " +
		"GainToHand purely as a 'move to hand' helper, not a real gain from the supply, and that's easy " +
		"to conflate.")]
	public void RevealedVictoryCardsDoNotTriggerGainHooks()
	{
		Assert.Inconclusive("Not implemented - no gain hooks exist yet to verify against.");
	}
}
