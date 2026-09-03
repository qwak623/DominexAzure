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
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);

		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);

		// selection is pulled from the candidate list itself, so this only succeeds if silver
		// ($3) genuinely passes the computed price threshold
		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(mine, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.SingleOrDefault(c => c.Card.Name == CardName.Silver));
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

		Assert.IsTrue(availableCards.Any(c => c.Card.Name == CardName.Silver));
		user.Verify(u => u.SelectCardToGain(mine, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(),
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.IsTreasure && x.Card.GetPrice(player.PlayerState) <= 3))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mine);

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
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);

		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand);
		// no treasure is available to gain
		EmptyKingdom();
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

		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomCopperToGold()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, throneRoom, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mine);
		var copperInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		var silverToGain = player.Game.Kingdom.GetPile(CardName.Silver).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == mineToPlay))).Returns(mineToPlay);
		user.SetupSequence(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copperInHand).Returns(silverToGain);

		// each resolution's selection is pulled from its own candidate list, so this only
		// succeeds if silver ($3) and gold ($6) genuinely pass their respective computed
		// price thresholds
		var expectedGains = new Queue<CardName>([CardName.Silver, CardName.Gold]);
		var seenCandidates = new List<List<CardInstance>>();
		user.Setup(u => u.SelectCardToGain(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
			{
				seenCandidates.Add(cards);
				var name = expectedGains.Dequeue();
				return cards.SingleOrDefault(x => x.Card.Name == name);
			});
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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

		user.Verify(u => u.SelectCardToGain(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.IsTreasure) && c.Max(x => x.Card.GetPrice(player.PlayerState)) == 3)), Times.Once);
		user.Verify(u => u.SelectCardToGain(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.IsTreasure) && c.Max(x => x.Card.GetPrice(player.PlayerState)) == 6)), Times.Once);

		Assert.IsTrue(seenCandidates[0].Any(c => c.Card.Name == CardName.Silver));
		Assert.IsTrue(seenCandidates[1].Any(c => c.Card.Name == CardName.Gold));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomTwoCoppersToSilvers()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([mine, throneRoom, copper, copper]);
		var mineToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Mine);
		var coppersInHand = player.PlayerState.Hand.Where(c => c.Card.Name == CardName.Copper).ToList();
		var copper1 = coppersInHand[0];
		var copper2 = coppersInHand[1];
		var silver1 = player.Game.Kingdom.GetPile(CardName.Silver).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == mineToPlay))).Returns(mineToPlay);
		user.SetupSequence(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(copper1).Returns(copper2);

		CardInstance silverToGain = null;
		user.Setup(u => u.SelectCardToGain(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
				silverToGain = cards.First(x => x.Card.Name == CardName.Silver))
			.Returns(() => silverToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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

		user.Verify(u => u.SelectCardToGain(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.IsTreasure && x.Card.GetPrice(player.PlayerState) <= 3))), Times.Exactly(2));
		#endregion
	}
}
