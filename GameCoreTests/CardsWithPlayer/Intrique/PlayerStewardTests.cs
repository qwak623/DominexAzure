using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerStewardTests : CardWithPlayerTestsBase
{
	private readonly Card steward = Steward.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(steward);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void ChoosesCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([steward]);
		player.PlayerState.DrawPile = CreatePile([silver, estate]);
		var stewardToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom, It.IsAny<List<StewardBenefit>>()))
			.Returns(StewardBenefit.Cards);
		#endregion

		#region act
		player.PlayActionCardInternal(stewardToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver, estate], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([steward], player.PlayerState.CardsPlayed);
		AssertPile([steward], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// the client is offered all three benefits to choose exactly one from
		user.Verify(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom,
			It.Is<List<StewardBenefit>>(b => b.Count == 3 && b.Contains(StewardBenefit.Cards)
				&& b.Contains(StewardBenefit.Coins) && b.Contains(StewardBenefit.Trash))), Times.Once);
		user.Verify(u => u.StewardChooseCardsToTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ChoosesCoins()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([steward]);
		var stewardToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom, It.IsAny<List<StewardBenefit>>()))
			.Returns(StewardBenefit.Coins);
		#endregion

		#region act
		player.PlayActionCardInternal(stewardToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.StewardChooseCardsToTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ChoosesTrashAndTrashesExactlyTwo()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([steward, silver, estate, copper]);
		var stewardToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Steward);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);
		var estateInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Estate);

		user.Setup(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom, It.IsAny<List<StewardBenefit>>()))
			.Returns(StewardBenefit.Trash);
		user.Setup(u => u.StewardChooseCardsToTrash(steward, player.PlayerState, player.Game.Kingdom, 2, It.IsAny<List<CardInstance>>()))
			.Returns([silverInHand, estateInHand]);
		#endregion

		#region act
		player.PlayActionCardInternal(stewardToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([silver, estate], player.Game.Trash);

		// mandatory exact-count trash: min and max are both 2 here, the same as Militia/
		// Torturer/Diplomat's mandatory discards - "trash 2 cards" isn't optional
		user.Verify(u => u.StewardChooseCardsToTrash(steward, player.PlayerState, player.Game.Kingdom, 2,
			It.Is<List<CardInstance>>(c => c.Count == 3)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ChoosesTrashWithFewerThanTwoCardsAvailable()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([steward, silver]);
		var stewardToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Steward);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		user.Setup(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom, It.IsAny<List<StewardBenefit>>()))
			.Returns(StewardBenefit.Trash);
		user.Setup(u => u.StewardChooseCardsToTrash(steward, player.PlayerState, player.Game.Kingdom, 1, It.IsAny<List<CardInstance>>()))
			.Returns([silverInHand]);
		#endregion

		#region act
		player.PlayActionCardInternal(stewardToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([silver], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ChoosesTrashWithNoCardsInHand()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([steward]);
		var stewardToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.StewardChooseBenefit(steward, player.PlayerState, player.Game.Kingdom, It.IsAny<List<StewardBenefit>>()))
			.Returns(StewardBenefit.Trash);
		user.Setup(u => u.StewardChooseCardsToTrash(steward, player.PlayerState, player.Game.Kingdom, 0, It.IsAny<List<CardInstance>>()))
			.Returns([]);
		#endregion

		#region act
		player.PlayActionCardInternal(stewardToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.Game.Trash);
		#endregion
	}
}
