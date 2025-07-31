using System.Xml.Linq;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using GameCore.GameCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class FeastTests : CardTestsBase
{
	private readonly Card feast = Feast.Get();
	private readonly Card duchy = Duchy.Get();

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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player has to trash feast
		CollectionAssert.AreEqual(new List<Card> { feast }, player.Object.Game.Trash.ToList());

		// the card was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain));

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
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player has to trash feast
		CollectionAssert.AreEqual(new List<Card> { feast }, player.Object.Game.Trash.ToList());

		// the feast was transferred from played cards to trash 
		Assert.IsFalse(player.Object.PlayerState.PlayedCards.Any());

		// user has to select a card with price max 5 to gain - there is none
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain));

		// player gains nothing
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}