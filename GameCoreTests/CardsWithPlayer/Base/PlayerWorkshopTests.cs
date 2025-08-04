using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerWorkshopTests : CardWithPlayerTestsBase
{
	private readonly Card workshop = Workshop.Get();
	private readonly Card village = Village.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { workshop, village });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainVillage()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { workshop };

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(village);
		#endregion

		#region act
		player.PlayActionCardInternal(workshop);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// player does not draw a card
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// user has to select a card with price max 4 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// player gains the village
		CollectionAssert.AreEquivalent(new List<Card> { village }, player.PlayerState.DiscardPile);

		// workshop was added to the played cards
		CollectionAssert.AreEquivalent(new List<Card> { workshop }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { workshop };

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(workshop);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// player does not draw a card
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// user has to select a card with price max 4 to gain - there is none
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 4 && k.OnlyTreasures == false), player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// player gains nothing
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// workshop was added to the played cards
		CollectionAssert.AreEquivalent(new List<Card> { workshop }, player.PlayerState.PlayedCards);
		#endregion
	}
}