using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerFestivalTests : CardWithPlayerTestsBase
{
	private readonly Card festival = Festival.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(festival);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { festival };
		#endregion

		#region act
		player.PlayActionCardInternal(festival);
		#endregion

		#region assert
		// (-1 Action, +2 Actions), +2 Coins, +1 Buy
		Assert.AreEqual(2, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(1, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// festival was added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { festival }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, festival };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == festival))).Returns(festival);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+2 Actions, +2 Coins, +1 Buy) * 2
		Assert.AreEqual(4, player.PlayerState.Actions);
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(2, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// festival and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { festival, throneRoom }, player.PlayerState.PlayedCards);
		#endregion
	}
}