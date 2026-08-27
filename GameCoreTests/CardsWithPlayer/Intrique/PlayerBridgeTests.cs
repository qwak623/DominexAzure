using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerBridgeTests : CardWithPlayerTestsBase
{
	private readonly Card bridge = Bridge.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card workshop = Workshop.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card duchy = Duchy.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(bridge);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PlayBridge()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([bridge]);
		var bridgeToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(bridgeToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 1, 1, player);
		Assert.AreEqual(1, player.PlayerState.TempEffects.GeneralCostReduction);

		AssertPile([], player.PlayerState.Hand);
		AssertPile([bridge], player.PlayerState.CardsPlayed);
		AssertPile([bridge], player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDoublesCostReduction()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, bridge]);
		var bridgeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Bridge);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == bridgeToPlay))).Returns(bridgeToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 2, 2, player);
		Assert.AreEqual(2, player.PlayerState.TempEffects.GeneralCostReduction);

		AssertPile([], player.PlayerState.Hand);
		AssertPile([throneRoom, bridge], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, bridge, bridge], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void CostReductionDoesNotGoBelowZero()
	{
		#region arrange
		// three separate bridges stack the reduction to 3, well past estate's own price of 2
		player.PlayerState.Hand = CreatePile([bridge, bridge, bridge]);
		var bridgesToPlay = player.PlayerState.Hand.ToList();
		#endregion

		#region act
		foreach (var bridgeToPlay in bridgesToPlay)
		{
			player.PlayActionCardInternal(bridgeToPlay);
		}
		#endregion

		#region assert
		Assert.AreEqual(3, player.PlayerState.TempEffects.GeneralCostReduction);
		Assert.AreEqual(0, estate.GetPrice(player.PlayerState));
		#endregion
	}

	[TestMethod]
	public void DiscountAppliesWhenBuying()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([bridge]);
		var bridgeToPlay = player.PlayerState.Hand[0];
		var silverToGain = player.Game.Kingdom.GetPile(CardName.Silver).CardInstance;

		player.PlayActionCardInternal(bridgeToPlay);

		user.Setup(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 1 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Buy)).Returns(silverToGain);
		#endregion

		#region act
		var bought = player.Buy();
		#endregion

		#region assert
		// silver normally costs 3, discounted by bridge's reduction to 2; player had 1 coin
		Assert.AreEqual(silverToGain, bought);
		Assert.AreEqual(0, player.PlayerState.Buys);
		Assert.AreEqual(-1, player.PlayerState.Coins);
		AssertPile([silver], player.PlayerState.DiscardPile);

		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 1 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Buy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AllowsWorkshopToGainCardCostingFiveAfterBridge()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([bridge, workshop]);
		var bridgeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Bridge);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Workshop);

		player.PlayActionCardInternal(bridgeToPlay);

		// selection is pulled from the wrapper itself, so this only succeeds if duchy
		// (normally $5) genuinely passes the wrapper's own availability check once
		// bridge's reduction brings its effective price down to workshop's $4 cap
		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardName.Duchy));
		#endregion

		#region act
		player.PlayActionCardInternal(workshopToPlay);
		#endregion

		#region assert
		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Name == CardName.Duchy));
		AssertPile([duchy], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void CostReductionResetsOnCleanup()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([bridge]);
		var bridgeToPlay = player.PlayerState.Hand[0];
		player.PlayActionCardInternal(bridgeToPlay);
		#endregion

		#region act
		player.Cleanup();
		#endregion

		#region assert
		Assert.AreEqual(0, player.PlayerState.TempEffects.GeneralCostReduction);
		#endregion
	}
}
