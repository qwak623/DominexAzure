using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerWishingWellTests : CardWithPlayerTestsBase
{
	private readonly Card wishingWell = WishingWell.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card gold = Gold.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(wishingWell);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GuessedCardMatchesRevealedCardAndMovesToHand()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([wishingWell]);
		// bottom to top: duchy is revealed by the guess mechanic once the +1 Card draw takes
		// copper off the top first
		player.PlayerState.DrawPile = CreatePile([duchy, copper]);
		var wishingWellToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.WishingWellGuess(wishingWell, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardType>>()))
			.Returns(CardType.Duchy);
		#endregion

		#region act
		player.PlayActionCardInternal(wishingWellToPlay);
		#endregion

		#region assert
		// +1 Action cancels out playing wishing well itself
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper, duchy], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([wishingWell], player.PlayerState.CardsPlayed);
		AssertPile([wishingWell], player.PlayerState.ActionsPlayed);

		// the guess is offered against every possible card type, not just this kingdom's own cards
		user.Verify(u => u.WishingWellGuess(wishingWell, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardType>>(types => types.Contains(CardType.Duchy) && types.Count == Enum.GetValues<CardType>().Length)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GuessedCardDoesNotMatchAndIsReturnedToTheTopOfTheDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([wishingWell]);
		player.PlayerState.DrawPile = CreatePile([duchy, copper]);
		var wishingWellToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.WishingWellGuess(wishingWell, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardType>>()))
			.Returns(CardType.Gold);
		#endregion

		#region act
		player.PlayActionCardInternal(wishingWellToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([duchy], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void StillGuessesEvenWhenNothingIsLeftToReveal()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([wishingWell]);
		// the single card is consumed by the +1 Card draw itself, leaving nothing to reveal
		player.PlayerState.DrawPile = CreatePile([copper]);
		var wishingWellToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.WishingWellGuess(wishingWell, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardType>>()))
			.Returns(CardType.Gold);
		#endregion

		#region act
		player.PlayActionCardInternal(wishingWellToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);

		// naming happens before the reveal, so it's still asked for even with nothing left to show
		user.Verify(u => u.WishingWellGuess(wishingWell, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardType>>()), Times.Once);
		#endregion
	}
}
