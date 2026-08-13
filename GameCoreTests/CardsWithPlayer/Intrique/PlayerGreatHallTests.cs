using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerGreatHallTests : CardWithPlayerTestsBase
{
	private readonly Card greatHall = GreatHall.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(greatHall);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PlayGreatHall()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([greatHall]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var greatHallToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(greatHallToPlay);
		#endregion

		#region assert
		// +1 Action, +1 Card
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([greatHall], player.PlayerState.CardsPlayed);
		AssertPile([greatHall], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void CountsAsOneVictoryPoint()
	{
		Assert.AreEqual(1, greatHall.CountPoints(player));
	}
}
