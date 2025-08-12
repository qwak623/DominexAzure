using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerFeastTests : CardWithPlayerTestsBase
{
	private readonly Card feast = Feast.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(feast);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainDuchy()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { feast };

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain)).Returns(duchy);
		#endregion

		#region act
		player.PlayActionCardInternal(feast);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// the feast was transferred from played cards to trash 
		Assert.IsFalse(player.PlayerState.PlayedCards.Any());

		// player gains the duchy to the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { duchy }, player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { feast };

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain)).Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(feast);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// the feast was transferred from played cards to trash 
		Assert.IsFalse(player.PlayerState.PlayedCards.Any());

		// player gains nothing
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainTwoDuchies()
	{
		// TODO hostina je dvakrát na smetišti

		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, feast };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(duchy);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// user has to select a card with price max 5 to gain two times
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains the duchy
		CollectionAssert.AreEquivalent(new List<Card> { duchy, duchy }, player.PlayerState.DiscardPile);

		// TODO je to dobře? 
		// throne room was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneDuchyAvailable()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, feast };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(duchy).Returns((Card)null);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// user has to select a card with price max 5 to gain - there is only one duchy
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains the one duchy
		CollectionAssert.AreEquivalent(new List<Card> { duchy }, player.PlayerState.DiscardPile);

		// TODO je to dobře? 
		// throne room was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, feast };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// user has to select a card with price max 5 to gain - there is none
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains nothing
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// TODO je to dobře?
		// throne room was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}
}