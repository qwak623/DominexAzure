using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerPatrolTests : CardWithPlayerTestsBase
{
	private readonly Card patrol = Patrol.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card curse = Curse.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(patrol);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PutsVictoryCardsAndCursesInHandAndOrdersTheRest()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([patrol]);
		// bottom to top: [copper, estate, silver, curse] are revealed by Patrol once the
		// three duchies (Patrol's own +3 Cards) are drawn off the top first
		player.PlayerState.DrawPile = CreatePile([copper, estate, silver, curse, duchy, duchy, duchy]);
		var patrolToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(patrolToPlay);
		#endregion

		#region assert
		// Patrol grants no actions/coins/buys of its own; the three duchies come from the
		// automatic +3 Cards draw, and estate/curse (victory and curse) go to hand as well,
		// leaving silver and copper (the two non-victory, non-curse cards revealed) to be
		// ordered back onto the deck
		AssertNumbers(0, 0, 0, player);
		AssertPile([duchy, duchy, duchy, estate, curse], player.PlayerState.Hand);
		AssertPile([silver, copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([patrol], player.PlayerState.CardsPlayed);
		AssertPile([patrol], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// only the two non-victory, non-curse cards are left to order once the rest are pulled
		// into hand
		user.Verify(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2 && c.All(x => !x.IsVictory && x.Card.Name != CardName.Curse))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NoVictoryOrCurseCardsAreLeftUntouched()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([patrol]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, copper, silver, duchy, duchy, duchy]);
		var patrolToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(patrolToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([duchy, duchy, duchy], player.PlayerState.Hand);
		AssertPile([copper, silver, copper, silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([patrol], player.PlayerState.CardsPlayed);
		AssertPile([patrol], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 4)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AllRevealedCardsAreVictoryOrCurse()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([patrol]);
		player.PlayerState.DrawPile = CreatePile([estate, curse, curse, curse, duchy, duchy, duchy]);
		var patrolToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(patrolToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([duchy, duchy, duchy, estate, curse, curse, curse], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([patrol], player.PlayerState.CardsPlayed);
		AssertPile([patrol], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// nothing left to order once every revealed card was a victory card or a curse
		user.Verify(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 0)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void RevealsFewerThanFourWhenDeckIsSmall()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([patrol]);
		// only two cards remain once the three duchies are drawn off for +3 Cards, so Patrol
		// can only reveal those two
		player.PlayerState.DrawPile = CreatePile([copper, estate, duchy, duchy, duchy]);
		var patrolToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards);
		#endregion

		#region act
		player.PlayActionCardInternal(patrolToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([duchy, duchy, duchy, estate], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([patrol], player.PlayerState.CardsPlayed);
		AssertPile([patrol], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 1)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AppliesTheChosenOrderToTheTopOfTheDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([patrol]);
		player.PlayerState.DrawPile = CreatePile([copper, silver, duchy, duchy, duchy]);
		var patrolToPlay = player.PlayerState.Hand[0];

		// put silver back first (bottom) and copper back last (top), regardless of the order
		// they were originally revealed in
		user.Setup(u => u.PatrolOrderCards(patrol, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
				[cards.Single(x => x.Card.Name == CardName.Silver), cards.Single(x => x.Card.Name == CardName.Copper)]);
		#endregion

		#region act
		player.PlayActionCardInternal(patrolToPlay);
		player.Draw(1);
		#endregion

		#region assert
		// copper was placed last in the chosen order, i.e. on top of the deck, so it's the
		// next card drawn
		AssertPile([duchy, duchy, duchy, copper], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	[Ignore("TODO: once gain hooks exist (see Player.GainToHand's 'TODO hook on gain event'), add a " +
		"test proving patrol's revealed victory/curse cards moving to hand do NOT trigger them - this " +
		"reuses GainToHand purely as a 'move to hand' helper, not a real gain from the supply, and that's " +
		"easy to conflate.")]
	public void RevealedVictoryAndCurseCardsDoNotTriggerGainHooks()
	{
		Assert.Inconclusive("Not implemented - no gain hooks exist yet to verify against.");
	}
}
