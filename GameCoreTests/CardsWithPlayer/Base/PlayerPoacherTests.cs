using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerPoacherTests : CardWithPlayerTestsBase
{
	private readonly Card poacher = Poacher.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card estate = Estate.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(poacher);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	// empties an entire kingdom pile without touching the player under test's own state
	private void DepleteKingdomPile(CardName type)
	{
		var depleter = CreatePlayer(game.Object, new Mock<IUser>().Object);
		while (!game.Object.Kingdom.GetPile(type).Empty)
		{
			depleter.Gain(type);
		}
	}

	[TestMethod]
	public void NothingIsDiscardedWhenNoSupplyPileIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([poacher, silver, estate]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var poacherToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Poacher);
		#endregion

		#region act
		player.PlayActionCardInternal(poacherToPlay);
		#endregion

		#region assert
		// +1 Action cancels out playing poacher itself; +$1 from poacher's own effect
		AssertNumbers(1, 1, 0, player);
		AssertPile([silver, estate, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([poacher], player.PlayerState.CardsPlayed);
		AssertPile([poacher], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.PoacherDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AsksToChooseWhenFewerCardsAreDiscardedThanTheWholeHand()
	{
		#region arrange
		DepleteKingdomPile(CardName.Province);

		player.PlayerState.Hand = CreatePile([poacher, silver, estate, gold]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var poacherToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Poacher);

		user.Setup(u => u.PoacherDiscard(poacher, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 4), 1)).Returns<Card, PlayerState, Kingdom, List<CardInstance>, int>(
			(c, ps, k, cards, count) => [cards.Single(x => x.Card.Name == CardName.Silver)]);
		#endregion

		#region act
		player.PlayActionCardInternal(poacherToPlay);
		#endregion

		#region assert
		AssertPile([estate, gold, copper], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DiscardPile);

		user.Verify(u => u.PoacherDiscard(poacher, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>(), 1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DiscardsTheWholeHandWithoutAskingWhenItDoesNotExceedTheEmptyPileCount()
	{
		#region arrange
		DepleteKingdomPile(CardName.Province);
		DepleteKingdomPile(CardName.Duchy);

		// only one card left in hand once poacher itself is played, and no draw pile to refill it
		player.PlayerState.Hand = CreatePile([poacher, estate]);
		player.PlayerState.DrawPile = CreatePile([]);
		var poacherToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Poacher);
		#endregion

		#region act
		player.PlayActionCardInternal(poacherToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([estate], player.PlayerState.DiscardPile);

		// two empty piles would normally mean discarding 2, but the hand only has 1 card left,
		// so there's nothing to choose between and PoacherDiscard is never asked
		user.Verify(u => u.PoacherDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}
}
