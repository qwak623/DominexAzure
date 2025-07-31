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
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// player does not draw a card
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
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain));

		// player does not draw a card
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// player has to trash feast
		CollectionAssert.AreEquivalent(new List<Card> { feast }, player.Game.Trash.ToList());

		// the feast was transferred from played cards to trash 
		Assert.IsFalse(player.PlayerState.PlayedCards.Any());

		// player gains nothing
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());
		#endregion
	}
}