using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerRemodelTests : CardWithPlayerTestsBase
{
	private readonly Card remodel = Remodel.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card laboratory = Laboratory.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { remodel, laboratory });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void UpgradeSilverToLaboratory()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { remodel, silver, laboratory };

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(silver);
		user.Setup(u => u.SelectCardToGain(
				It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
				player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(laboratory);
		#endregion

		#region act
		player.PlayActionCardInternal(remodel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose a card to trash
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes the chosen card
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player.Game.Trash);

		// user is asked to select a card with max price 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		// no card is drawn to hand
		CollectionAssert.AreEquivalent(new List<Card>() { laboratory }, player.PlayerState.Hand);

		// laboratory is gained
		CollectionAssert.AreEquivalent(new List<Card>() { laboratory }, player.PlayerState.DiscardPile);

		// remodel was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { remodel }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { remodel, silver, laboratory };

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom)).Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(remodel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose a card to trash
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player does not trash anything
		Assert.IsFalse(player.Game.Trash.Any());

		// user is never asked to choose any card to gain
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);

		// no card is drawn to hand
		CollectionAssert.AreEquivalent(new List<Card>() { silver, laboratory }, player.PlayerState.Hand);

		// nothing is gained
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// remodel was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { remodel }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { remodel, silver, laboratory };

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(laboratory);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(remodel);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose a card to trash
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes the chosen card
		CollectionAssert.AreEquivalent(new List<Card> { laboratory }, player.Game.Trash);

		// user is asked to select a card with max price 7 to gain, but there is no such card available
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 7 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		// no card was drawn to hand
		CollectionAssert.AreEquivalent(new List<Card>() { silver }, player.PlayerState.Hand);

		// nothing is gained
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// remodel was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { remodel }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { remodel, silver, throneRoom, throneRoom };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(remodel)))).Returns(remodel);
		user.SetupSequence(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(silver).Returns(throneRoom);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(laboratory).Returns(gold);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose a card to trash two times
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));

		// player trashes the chosen cards
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// user is asked to select a card with max price 5 or 6 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 6 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		// laboratory and gold are gained
		CollectionAssert.AreEquivalent(new List<Card> { laboratory, gold }, player.PlayerState.DiscardPile);

		// remodel and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { remodel, throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}
}