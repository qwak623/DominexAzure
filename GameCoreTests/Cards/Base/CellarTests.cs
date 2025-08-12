using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class CellarTests : CardTestsBase
{
	private readonly Card cellar = Cellar.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(cellar));
	}

	[TestMethod]
	public void DrawNoCards()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player does not discard any card
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player does not draw any card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DrawOneCard()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards the chosen card
		player.Verify(p => p.Discard(copper), Times.Once);

		// player draws one card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DrawFourCards()
	{
		#region arrange
		player.Setup(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, cellar, copper });
		#endregion

		#region act
		cellar.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player discards the chosen cards
		player.Verify(p => p.Discard(copper), Times.Exactly(3));
		player.Verify(p => p.Discard(cellar), Times.Once);

		// player draws four cards
		player.Verify(p => p.Draw(4), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { cellar };
		player.SetupSequence(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(new List<Card> { copper, copper, cellar, copper }).Returns(new List<Card> { cellar, copper });

		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(cellar)))).Returns(cellar);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// (+1 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +4 Cards for the first cellar, +2 Cards for the second one
		player.Verify(p => p.Draw(4), Times.Once);
		player.Verify(p => p.Draw(2), Times.Once);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose cards to discard
		player.Verify(p => p.User.CellarDiscard(cellar, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Exactly(2));

		// player discards the chosen cards
		player.Verify(p => p.Discard(copper), Times.Exactly(4));
		player.Verify(p => p.Discard(cellar), Times.Exactly(2));
		#endregion
	}
}