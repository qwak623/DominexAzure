using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class FeastTests : CardTestsBase
{
	private readonly Card feast = Feast.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(feast));
		player.Object.PlayerState.PlayedCards = new List<Card> { feast };
		player.Setup(p => p.Game.Trash).Returns(new List<Card>());
	}

	[TestMethod]
	public void GainDuchy()
	{
		#region arrange
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(duchy);
		#endregion

		#region act
		feast.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Object.Game.Trash);

		// the card was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// player gains the duchy
		player.Verify(p => p.Gain(CardType.Duchy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		feast.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Object.Game.Trash);

		// the feast was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainTwoDuchies()
	{
		// TODO hostina je dvakrát na smetišti

		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { feast };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(duchy);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Object.Game.Trash);

		// the card was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain two times
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains the duchy
		player.Verify(p => p.Gain(CardType.Duchy), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneDuchyAvailable()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { feast };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		player.SetupSequence(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(duchy).Returns((Card)null);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Object.Game.Trash);

		// the card was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain - there is only one duchy
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains the one duchy
		player.Verify(p => p.Gain(CardType.Duchy), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { feast };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == feast))).Returns(feast);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Object.Game.Trash);

		// the card was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}