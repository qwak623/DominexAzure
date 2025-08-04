using System.Threading.Channels;
using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerSmithyTests : CardWithPlayerTestsBase
{
	private readonly Card smithy = Smithy.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(smithy);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { smithy };
		player.PlayerState.DrawPile = new List<Card> { copper, copper, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(smithy);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +3 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.PlayerState.DrawPile.Any());

		// smithy was added to played cards
		CollectionAssert.AreEqual(new List<Card> { smithy }, player.PlayerState.PlayedCards);
		#endregion
	}
}