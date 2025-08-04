using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerWoodcutterTests : CardWithPlayerTestsBase
{
	private readonly Card woodcutter = Woodcutter.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(woodcutter);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { woodcutter };
		#endregion

		#region act
		player.PlayActionCardInternal(woodcutter);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +2 Coins
		Assert.AreEqual(2, player.PlayerState.Coins);

		// +1 Buy
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// woodcutter was added to played cards
		CollectionAssert.AreEqual(new List<Card> { woodcutter }, player.PlayerState.PlayedCards);
		#endregion
	}
}