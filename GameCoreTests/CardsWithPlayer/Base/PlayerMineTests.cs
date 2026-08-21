using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMineTests : CardWithPlayerTestsBase
{
	private readonly Card mine = Mine.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(mine);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void UpgradeCopperToSilver()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper);

		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);

		// selection is pulled from the wrapper itself, so this only succeeds if silver ($3)
		// genuinely passes the wrapper's own availability check at the computed threshold
		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Silver));
		#endregion

		#region act
		player.PlayActionCardInternal(mineToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mine], player.PlayerState.CardsPlayed);
		AssertPile([mine], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Silver));
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 3 && kw.OnlyTreasures),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Mine);

		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(mineToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mine], player.PlayerState.CardsPlayed);
		AssertPile([mine], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper);

		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(mineToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mine], player.PlayerState.CardsPlayed);
		AssertPile([mine], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomCopperToGold()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, throneRoom, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper);
		var silverToGain = player.Game.Kingdom.GetPile(CardType.Silver).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == mineToPlay))).Returns(mineToPlay);
		user.SetupSequence(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand).Returns(silverToGain);

		// each resolution's selection is pulled from its own wrapper, so this only succeeds
		// if silver ($3) and gold ($6) genuinely pass the wrapper's own availability check
		// at their respective computed thresholds
		var expectedGains = new Queue<CardType>([CardType.Silver, CardType.Gold]);
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
		AssertNumbers(0, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mine, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([mine, mine, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([copper, silver], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// second resolution's selection is the silver the first resolution just gained - it goes
		// straight back in for a gold
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.SequenceEqual(new List<CardInstance> { copperInHand }))), Times.Once);
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.SequenceEqual(new List<CardInstance> { silverToGain }))), Times.Once);

		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 3 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 6 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		Assert.IsTrue(wrappers[0].AvailableCards.Any(c => c.Card.Type == CardType.Silver));
		Assert.IsTrue(wrappers[1].AvailableCards.Any(c => c.Card.Type == CardType.Gold));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomTwoCoppersToSilvers()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, throneRoom, copper, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Mine);
		var coppersInHand = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).ToList();
		var copper1 = coppersInHand[0];
		var copper2 = coppersInHand[1];
		var silver1 = player.Game.Kingdom.GetPile(CardType.Silver).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == mineToPlay))).Returns(mineToPlay);
		user.SetupSequence(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copper1).Returns(copper2);

		CardInstance silverToGain = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) =>
				silverToGain = kw.AvailableCards.First(c => c.Card.Type == CardType.Silver))
			.Returns(() => silverToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([mine, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([mine, mine, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([copper, copper], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// first resolution offers both starting coppers; second offers whichever wasn't trashed
		// plus the silver the first resolution just gained
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.SequenceEqual(new List<CardInstance> { copper1, copper2 }))), Times.Once);
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.SequenceEqual(new List<CardInstance> { copper2, silver1 }))), Times.Once);

		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.MaxPrice == 3 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}
}
