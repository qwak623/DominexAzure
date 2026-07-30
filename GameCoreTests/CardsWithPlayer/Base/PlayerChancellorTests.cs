using System.ComponentModel;
using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerChancellorTests : CardWithPlayerTestsBase
{
	private readonly Card chancellor = Chancellor.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(chancellor);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(true);

		player.PlayerState.Hand = new List<Card> { chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.PlayActionCardInternal(chancellor);
		#endregion

		#region assert
		// -1 Action, +2 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player discards his draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.DiscardPile);

		// chancellor was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { chancellor }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { chancellor }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void DontDiscardDrawPile()
	{
		#region arrange
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom)).Returns(false);

		player.PlayerState.Hand = new List<Card> { chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };
		#endregion

		#region act
		player.PlayActionCardInternal(chancellor);
		#endregion

		#region assert
		// -1 Action, +2 Coins, +0 Buys
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(2, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Once);

		// player doesn't discard his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DiscardPile);

		// chancellor was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { chancellor }, player.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { chancellor }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	[DataRow(true, true, DisplayName = "DiscardsBothTimes")]
	[DataRow(true, false, DisplayName = "OnlyDiscardsForTheFirstTime")]
	[DataRow(false, true, DisplayName = "OnlyDiscardsForTheSecondTime")]
	public void ThroneRoomDiscardDrawPile(bool discardFirstTime, bool discardSecondTime)
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == chancellor))).Returns(chancellor);
		user.SetupSequence(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));

		// player discards his draw pile
		Assert.IsFalse(player.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, silver }, player.PlayerState.DiscardPile);

		// chancellor and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { chancellor, throneRoom }, player.PlayerState.CardsPlayed);

		// two chancellors and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { chancellor, chancellor, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDontDiscardDrawPile()
	{
		#region arrange
		player.PlayerState.Hand = new List<Card> { throneRoom, chancellor, copper };
		player.PlayerState.DiscardPile = new List<Card> { copper };
		player.PlayerState.DrawPile = new List<Card> { silver, silver };

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == chancellor))).Returns(chancellor);
		user.Setup(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(4, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// +0 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.Hand);

		// user is asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose whether to discard his draw pile
		user.Verify(u => u.ChancellorDiscard(chancellor, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));

		// player doesn't discard his draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver, silver }, player.PlayerState.DrawPile);
		CollectionAssert.AreEquivalent(new List<Card> { copper }, player.PlayerState.DiscardPile);

		// chancellor and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { chancellor, throneRoom }, player.PlayerState.CardsPlayed);

		// two chancellors and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { chancellor, chancellor, throneRoom }, player.PlayerState.ActionsPlayed);
		#endregion
	}
}