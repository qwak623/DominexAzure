#if false
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class ChapelTests : CardTestsBase
{
	private readonly Card chapel = Chapel.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(chapel));
		player.Object.PlayerState.Hand = new List<Card> { copper, silver, silver, copper, chapel };
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player does not trash anything
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);
		#endregion
	}
	[TestMethod]
	public void TrashOneCard()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the card
		player.Verify(p => p.Trash(copper), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TrashFourCards()
	{
		#region arrange
		player.Setup(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver });
		#endregion

		#region act
		chapel.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose cards to trash
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the cards
		player.Verify(p => p.Trash(copper), Times.Exactly(2));
		player.Verify(p => p.Trash(silver), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoom4Treasures()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { chapel };

		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(chapel)))).Returns(chapel);
		player.SetupSequence(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, silver, silver })
			.Returns(new List<Card> { copper, copper });
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose cards to trash twice
		player.Verify(p => p.User.ChapelTrash(chapel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Exactly(2));

		// player trashes the cards
		player.Verify(p => p.Trash(copper), Times.Exactly(4));
		player.Verify(p => p.Trash(silver), Times.Exactly(2));
		#endregion
	}
}
#endif
