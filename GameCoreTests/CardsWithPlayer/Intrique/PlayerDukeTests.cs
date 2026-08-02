using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerDukeTests : CardWithPlayerTestsBase
{
	private readonly Card duke = Duke.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(duke);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void CountPoints()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([duke, duchy, copper, copper]);
		player.PlayerState.DrawPile = CreatePile([duke, duchy, copper]);
		player.PlayerState.DiscardPile = CreatePile([duchy, copper, copper]);

		game.Setup(g => g.GameEnd).Returns(true);
		player.FinalCleanup();
		#endregion

		#region act
		var points = player.GetVictoryPoints();
		#endregion

		#region assert
		// 2 Dukes, each worth 1 VP per Duchy (3 Duchies) = 6, plus 3 Duchies worth 3 VP each = 9
		Assert.AreEqual(15, points);
		#endregion
	}
}
