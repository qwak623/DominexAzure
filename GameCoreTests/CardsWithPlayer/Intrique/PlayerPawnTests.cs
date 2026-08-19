using GameCore.Cards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerPawnTests : CardWithPlayerTestsBase
{
	private readonly Card pawn = Pawn.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(pawn);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void CardAndAction()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([pawn]);
		player.PlayerState.DrawPile = CreatePile([pawn]);
		var pawnToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PawnChooseBenefits(pawn, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<PawnBenefit>>()))
			.Returns([PawnBenefit.Card, PawnBenefit.Action]);
		#endregion

		#region act
		player.PlayActionCardInternal(pawnToPlay);
		#endregion

		#region assert
		// the drawn card cancels out the drawpile-emptying, and +1 action cancels out the
		// action spent playing pawn itself
		AssertNumbers(1, 0, 0, player);
		AssertPile([pawn], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([pawn], player.PlayerState.CardsPlayed);
		AssertPile([pawn], player.PlayerState.ActionsPlayed);

		// the client is offered all four benefits to choose exactly two, distinct, from
		user.Verify(u => u.PawnChooseBenefits(pawn, player.PlayerState, player.Game.Kingdom, 2,
			It.Is<List<PawnBenefit>>(b => b.Count == 4 && b.Contains(PawnBenefit.Card) && b.Contains(PawnBenefit.Action)
				&& b.Contains(PawnBenefit.Buy) && b.Contains(PawnBenefit.Coin))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void BuyAndCoin()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([pawn]);
		var pawnToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PawnChooseBenefits(pawn, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<PawnBenefit>>()))
			.Returns([PawnBenefit.Buy, PawnBenefit.Coin]);
		#endregion

		#region act
		player.PlayActionCardInternal(pawnToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 1, 1, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([pawn], player.PlayerState.CardsPlayed);
		AssertPile([pawn], player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void CardAndCoin()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([pawn]);
		player.PlayerState.DrawPile = CreatePile([pawn]);
		var pawnToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PawnChooseBenefits(pawn, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<PawnBenefit>>()))
			.Returns([PawnBenefit.Card, PawnBenefit.Coin]);
		#endregion

		#region act
		player.PlayActionCardInternal(pawnToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 1, 0, player);
		AssertPile([pawn], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void ActionAndBuy()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([pawn]);
		var pawnToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.PawnChooseBenefits(pawn, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<PawnBenefit>>()))
			.Returns([PawnBenefit.Action, PawnBenefit.Buy]);
		#endregion

		#region act
		player.PlayActionCardInternal(pawnToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 1, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		#endregion
	}
}
