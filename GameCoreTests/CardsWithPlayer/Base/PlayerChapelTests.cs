using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		player.PlayerState.Hand = new List<Card> { copper, silver, silver, copper, chapel };
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom)).Returns(new List<Card> { });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver, copper }, player.PlayerState.Hand);
		Assert.IsFalse(player.Game.Trash.Any());

		// chapel was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.ActionsPlayed);
		#endregion
	}
	[TestMethod]
	public void TrashOneCard()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom)).Returns(new List<Card> { copper });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes a copper
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.Game.Trash);

		// chapel was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void TrashFourCards()
	{
		#region arrange
		user.Setup(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver });
		#endregion

		#region act
		player.PlayActionCardInternal(chapel);
		#endregion

		#region assert
		// -1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// user is asked to choose cards to trash
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player trashes four cards
		Assert.IsFalse(player.PlayerState.Hand.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver, copper }, player.Game.Trash);

		// chapel was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { chapel }, player.PlayerState.ActionsPlayed);
		#endregion
	}


	[TestMethod]
	public void ThroneRoomTwoCoppersToSilvers()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { chapel, throneRoom, copper, copper, silver, silver, copper, copper };
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == chapel))).Returns(chapel);
		user.SetupSequence(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver })
			.Returns(new List<Card> { copper, copper });
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		Assert.IsFalse(player.PlayerState.DiscardPile.Any());
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose cards to trash twice
		user.Verify(u => u.ChapelTrash(chapel, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));

		// player trashes the chosen cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, silver, silver, copper, copper }, player.Game.Trash);

		// chapel and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { chapel, throneRoom }, player.PlayerState.CardsPlayed);

		// two chapels and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { chapel, chapel, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}