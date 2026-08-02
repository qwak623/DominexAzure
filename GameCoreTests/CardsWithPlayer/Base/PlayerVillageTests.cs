using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerVillageTests : CardWithPlayerTestsBase
{
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

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
		player.PlayerState.Hand = CreatePile([village]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var villageToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(villageToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([village], player.PlayerState.CardsPlayed);
		AssertPile([village], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, village]);
		player.PlayerState.DrawPile = CreatePile([copper, copper]);
		var villageToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Village);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == villageToPlay))).Returns(villageToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(4, 0, 0, player);
		AssertPile([copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([village, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([village, village, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}
}
