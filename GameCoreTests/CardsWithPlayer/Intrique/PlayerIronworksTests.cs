using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerIronworksTests : CardWithPlayerTestsBase
{
	private readonly Card ironworks = Ironworks.Get();
	private readonly Card village = Village.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card harem = Harem.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([ironworks, village, harem]);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainAction()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([ironworks]);
		var ironworksToPlay = player.PlayerState.Hand[0];

		// selection is pulled from the wrapper itself, so this only succeeds if village
		// genuinely passes the wrapper's own availability check
		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Village));
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		// gaining an action card grants +1 action - village's own bonuses don't apply, it's gained, not played
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([village], player.PlayerState.DiscardPile);
		AssertPile([ironworks], player.PlayerState.CardsPlayed);
		AssertPile([ironworks], player.PlayerState.ActionsPlayed);

		Assert.AreEqual(4, wrapper.Price);
		Assert.IsFalse(wrapper.OnlyTreasures);
		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Village));
		#endregion
	}

	[TestMethod]
	public void GainTreasure()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([ironworks]);
		var ironworksToPlay = player.PlayerState.Hand[0];

		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Silver));
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 1, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DiscardPile);
		AssertPile([ironworks], player.PlayerState.CardsPlayed);
		AssertPile([ironworks], player.PlayerState.ActionsPlayed);

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Silver));
		#endregion
	}

	[TestMethod]
	public void GainVictory()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([ironworks]);
		player.PlayerState.DrawPile = CreatePile([silver]);
		var ironworksToPlay = player.PlayerState.Hand[0];

		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Estate));
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		// gaining a victory card grants +1 card
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([estate], player.PlayerState.DiscardPile);
		AssertPile([ironworks], player.PlayerState.CardsPlayed);
		AssertPile([ironworks], player.PlayerState.ActionsPlayed);

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Estate));
		#endregion
	}

	[TestMethod]
	public void GainTreasureAndVictory()
	{
		#region arrange
		// harem is both a treasure and a victory card, so both bonuses should fire; a cost
		// reduction of 2 (as bridge would provide) brings its price down from $6 to $4 so
		// it clears ironworks' own cap
		player.PlayerState.Hand = CreatePile([ironworks]);
		player.PlayerState.DrawPile = CreatePile([silver]);
		player.PlayerState.TempEffects.ReduceCost(2);
		var ironworksToPlay = player.PlayerState.Hand[0];

		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Harem));
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		// +1 Coin (treasure) and +1 Card (victory)
		AssertNumbers(0, 1, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([harem], player.PlayerState.DiscardPile);
		AssertPile([ironworks], player.PlayerState.CardsPlayed);
		AssertPile([ironworks], player.PlayerState.ActionsPlayed);

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Harem));
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([ironworks]);
		var ironworksToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([ironworks], player.PlayerState.CardsPlayed);
		AssertPile([ironworks], player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void AvailableCardsAreLimitedToPriceFour()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([ironworks]);
		var ironworksToPlay = player.PlayerState.Hand[0];

		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => availableCards = kw.AvailableCards.ToList())
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(ironworksToPlay);
		#endregion

		#region assert
		// village ($3) and silver ($3) are within ironworks' cap, harem ($6) and duchy ($5) are not
		Assert.IsTrue(availableCards.Any(c => c.Card.Type == CardType.Village));
		Assert.IsTrue(availableCards.Any(c => c.Card.Type == CardType.Silver));
		Assert.IsFalse(availableCards.Any(c => c.Card.Type == CardType.Harem));
		Assert.IsFalse(availableCards.Any(c => c.Card.Type == CardType.Duchy));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainsActionAndTreasure()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, ironworks]);
		var ironworksToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Ironworks);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == ironworksToPlay))).Returns(ironworksToPlay);

		var expectedGains = new Queue<CardType>([CardType.Village, CardType.Silver]);
		var wrappers = new List<KingdomWrapper>();
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) =>
			{
				wrappers.Add(kw);
				return kw.GetCard(expectedGains.Dequeue());
			});
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// first resolution gains village (+1 Action), second gains silver (+1 Coin)
		AssertNumbers(1, 1, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([village, silver], player.PlayerState.DiscardPile);
		AssertPile([ironworks, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([ironworks, ironworks, throneRoom], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		Assert.AreEqual(2, wrappers.Count);
		Assert.IsTrue(wrappers[0].AvailableCards.Any(c => c.Card.Type == CardType.Village));
		Assert.IsTrue(wrappers[1].AvailableCards.Any(c => c.Card.Type == CardType.Silver));
		#endregion
	}
}
