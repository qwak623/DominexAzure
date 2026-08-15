using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerChapelTests : CardWithPlayerTestsBase
{
	private readonly Card chapel = Chapel.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(chapel);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
		player.PlayerState.Hand = CreatePile([copper, silver, silver, copper, chapel]);
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		var chapelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chapel);
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>())).Returns([]);
		#endregion

		#region act
		player.PlayActionCardInternal(chapelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, silver, silver, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([chapel], player.PlayerState.CardsPlayed);
		AssertPile([chapel], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TrashOneCard()
	{
		#region arrange
		var chapelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chapel);
		var copperToTrash = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Copper);
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>())).Returns([copperToTrash]);
		#endregion

		#region act
		player.PlayActionCardInternal(chapelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([chapel], player.PlayerState.CardsPlayed);
		AssertPile([chapel], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TrashFourCards()
	{
		#region arrange
		var chapelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chapel);
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns([.. player.PlayerState.Hand.Where(c => c.Card.Type != CardType.Chapel)]);
		#endregion

		#region act
		player.PlayActionCardInternal(chapelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([chapel], player.PlayerState.CardsPlayed);
		AssertPile([chapel], player.PlayerState.ActionsPlayed);
		AssertPile([copper, silver, silver, copper], player.Game.Trash);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomTwoCoppersToSilvers()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([chapel, throneRoom, copper, copper, silver, silver, copper, copper]);
		var chapelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Chapel);
		var coppers = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).ToList();
		var silvers = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.Silver).ToList();

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == chapelToPlay))).Returns(chapelToPlay);
		user.SetupSequence(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns([coppers[0], coppers[1], silvers[0], silvers[1]])
			.Returns([coppers[2], coppers[3]]);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([chapel, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([chapel, chapel, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([copper, copper, silver, silver, copper, copper], player.Game.Trash);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// user is asked to choose cards to trash twice
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Exactly(2));
		#endregion
	}
}
