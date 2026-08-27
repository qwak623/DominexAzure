using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerSmithyTests : CardWithPlayerTestsBase
{
	private readonly Card smithy = Smithy.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

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
		player.PlayerState.Hand = CreatePile([smithy]);
		player.PlayerState.DrawPile = CreatePile([copper, copper, copper]);
		var smithyToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(smithyToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([smithy], player.PlayerState.CardsPlayed);
		AssertPile([smithy], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, smithy]);
		player.PlayerState.DrawPile = CreatePile([copper, copper, copper, copper, copper, copper]);
		var smithyToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Smithy);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == smithyToPlay))).Returns(smithyToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, copper, copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([smithy, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([smithy, smithy, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}
}
