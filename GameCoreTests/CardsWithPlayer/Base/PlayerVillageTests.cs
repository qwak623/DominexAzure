using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerVillageTests : CardWithPlayerTestsBase
{
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(village);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { village };
		player.PlayerState.DrawPile = new List<Card> { copper };
		#endregion

		#region act
		player.PlayActionCardInternal(village);
		#endregion

		#region assert
		// (-1 Action, +2 Actions)
		Assert.AreEqual(2, player.PlayerState.Actions);

		// +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// village was added to played cards
		CollectionAssert.AreEqual(new List<Card> { village }, player.PlayerState.PlayedCards);
		#endregion
	}
}