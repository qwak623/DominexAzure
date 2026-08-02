using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerAdventurerTests : CardWithPlayerTestsBase
{
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card province = Province.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(adventurer);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DrawTwoTreasures()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper]);
		player.PlayerState.DrawPile = CreatePile([copper, silver]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([adventurer], player.PlayerState.CardsPlayed);
		AssertPile([adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void SkipNonTreasures()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper]);
		player.PlayerState.DrawPile = CreatePile([province, gold, silver, adventurer, adventurer, province]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, silver, gold], player.PlayerState.Hand);
		AssertPile([province], player.PlayerState.DrawPile);
		AssertPile([province, adventurer, adventurer], player.PlayerState.DiscardPile);
		AssertPile([adventurer], player.PlayerState.CardsPlayed);
		AssertPile([adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void OneTreasureToDraw()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper]);
		player.PlayerState.DrawPile = CreatePile([gold, province]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		// the non-treasure card is on the discard pile
		// player has the treasure in his hand
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([province], player.PlayerState.DiscardPile);
		AssertPile([adventurer], player.PlayerState.CardsPlayed);
		AssertPile([adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void NoTreasuresToDraw()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, copper]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		// nothing was added to the player's hand
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([adventurer], player.PlayerState.CardsPlayed);
		AssertPile([adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomFourTreasures()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, throneRoom]);
		var adventurerToPlay = player.PlayerState.Hand[0];
		player.PlayerState.DrawPile =
			CreatePile([silver, province, copper, province, province, gold, province, silver, adventurer, province, copper]);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Contains(adventurerToPlay)))).Returns(adventurerToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[1]);
		#endregion

		#region assert
		// player gained the treasures to the hand
		// non-treasure cards were discarded
		// throne room and adventurer were added to played cards
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, silver, gold, copper], player.PlayerState.Hand);
		AssertPile([province, silver], player.PlayerState.DrawPile);
		AssertPile([province, province, province, adventurer, province], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, adventurer], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, adventurer, adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(3)]
	public void ThroneRoomNotEnoughTreasures(int treasureCount)
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([adventurer, throneRoom]);
		var adventurerToPlay = player.PlayerState.Hand[0];
		player.PlayerState.DrawPile =
			CreatePile([province, province, province, province, adventurer, province, .. Enumerable.Repeat(gold, treasureCount)]);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Contains(adventurerToPlay)))).Returns(adventurerToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[1]);
		#endregion

		#region assert
		// player gained the treasures to the hand
		// non-treasure cards were discarded
		AssertNumbers(0, 0, 0, player);
		AssertPile([.. Enumerable.Repeat(gold, treasureCount)], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([province, province, province, province, adventurer, province], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, adventurer], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, adventurer, adventurer], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}
}