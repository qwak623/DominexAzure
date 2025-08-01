using System.Threading.Channels;
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
		CollectionAssert.AreEqual(new List<Card> { festival }, player.PlayerState.PlayedCards);
		#endregion
	}
}