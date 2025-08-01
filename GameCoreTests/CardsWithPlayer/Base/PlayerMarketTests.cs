using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMarketTests : CardWithPlayerTestsBase
{
	private readonly Card market = Market.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(market);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { market };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.PlayActionCardInternal(market);
		#endregion

		#region assert
		// (-1 Action, +1 Actions)
		Assert.AreEqual(1, player.PlayerState.Actions);

		// +1 Coins, +1 Buys
		Assert.AreEqual(1, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// market was added to played cards
		CollectionAssert.AreEqual(new List<Card> { market }, player.PlayerState.PlayedCards);
		#endregion
	}
}