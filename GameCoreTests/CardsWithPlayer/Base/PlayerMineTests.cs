using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns(silver);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// copper was trashed
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.Game.Trash);

		// silver was gained to hand
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.PlayerState.Hand);

		// mine was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns<Card>(null);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// nothing was trashed
		Assert.IsFalse(player.Game.Trash.Any());

		// copper stayed in hand
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// mine was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns<Card>(null);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// copper was trashed
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.Game.Trash);

		// nothing was gained to hand
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// mine was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { mine }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomCopperToGold()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { mine, throneRoom, copper };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == mine))).Returns(mine);
		user.SetupSequence(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns(copper).Returns(silver);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(silver).Returns(gold);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose a treasure to trash
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { silver }))), Times.Once);

		// player trashes the chosen cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, player.Game.Trash);

		// gold is added to the hand
		CollectionAssert.AreEquivalent(new List<Card> { gold }, player.PlayerState.Hand);

		// user is asked to select a treasure with max price 3 or 6 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 6 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		// mine and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { mine, throneRoom }, player.PlayerState.CardsPlayed);

		// two mines and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { mine, mine, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomTwoCoppersToSilvers()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { mine, throneRoom, copper, copper };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == mine))).Returns(mine);
		user.Setup(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(silver);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose a treasure to trash
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper, copper }))), Times.Once);
		user.Verify(u => u.MineTrash(mine, player.PlayerState, player.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper, silver }))), Times.Once);

		// player trashes the chosen cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, player.Game.Trash);

		// silvers are added to the hand
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, player.PlayerState.Hand);

		// user is asked to select a treasure with max price 3 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// mine and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { mine, throneRoom }, player.PlayerState.CardsPlayed);

		// two mines and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { mine, mine, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}