using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerChancellorTests : CardWithPlayerTestsBase
{
	private readonly Card chancellor = Chancellor.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(chancellor);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(true);

		player.PlayerState.Hand = new List<Card> { chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.PlayActionCardInternal(chancellor);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +2 Coins
		Assert.AreEqual(2, player.PlayerState.Coins);

		// Buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEqual(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player discards his draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.DiscardPile);

		// chancellor was added to played cards
		CollectionAssert.AreEqual(new List<Card> { chancellor }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DontDiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(false);

		player.PlayerState.Hand = new List<Card> { chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.PlayActionCardInternal(chancellor);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// +2 Coins
		Assert.AreEqual(2, player.PlayerState.Coins);

		// Buys shouldn't change 
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEqual(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player doesn't discard his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DiscardPile);

		// chancellor was added to played cards
		CollectionAssert.AreEqual(new List<Card> { chancellor }, player.PlayerState.PlayedCards);
		#endregion
	}
}