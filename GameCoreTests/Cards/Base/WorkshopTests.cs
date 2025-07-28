using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class WorkshopTests : CardTestsBase
{
	private readonly Card workshop = Workshop.Get();
	private readonly Card village = Village.Get();

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
		workshop.WhenPlayAction(player.Object);

		#endregion
		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user has to select a card with price max 4 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain));

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
		workshop.WhenPlayAction(player.Object);

		#endregion
		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user has to select a card with price max 5 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain));

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}