using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerGardensTests : CardWithPlayerTestsBase
{
	private readonly Card gardens = Gardens.Get();
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(gardens);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void CountPoints()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { gardens, copper, copper, copper, copper };
		player.PlayerState.DrawPile = new List<Card> { adventurer, copper, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper, copper, gardens, adventurer, copper };

		game.Setup(g => g.GameEnd).Returns(true);
		#endregion

		#region act
		var points = player.VictoryPoints;
		#endregion

		#region assert
		Assert.AreEqual(2, points);
		#endregion
	}
}