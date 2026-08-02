#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class WorkshopTests : CardTestsBase
{
	private readonly Card workshop = Workshop.Get();
	private readonly Card village = Village.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(workshop));
	}

	[TestMethod]
	public void GainVillage()
	{
		#region arrange
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(village);
		#endregion

		#region act
		workshop.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user has to select a card with price max 4 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// player gains the village
		player.Verify(p => p.Gain(CardType.Village), Times.Once);
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
		workshop.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user has to select a card with price max 4 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainTwoVillages()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { workshop };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == workshop))).Returns(workshop);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(village);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
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

		// user has to select a card with price max 4 to gain two times
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains two villages
		player.Verify(p => p.Gain(CardType.Village), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneVillageAvailable()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { workshop };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == workshop))).Returns(workshop);
		player.SetupSequence(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(village).Returns((Card)null);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
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

		// user has to select a card with price max 4 to gain - there is only one village
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains the one village
		player.Verify(p => p.Gain(CardType.Village), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { workshop };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == workshop))).Returns(workshop);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
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

		// user has to select a card with price max 4 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}
#endif
