using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class CouncilRoomTests : CardTestsBase
{
	private readonly Card councilRoom = CouncilRoom.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Kingdom kingdom;
	private Mock<IPlayer> player;
	private Mock<IPlayer> player2;
	private Mock<IPlayer> player3;
	private Mock<IPlayer> player4;

	[TestInitialize]
	public void Init()
	{
		kingdom = MockKingdom(councilRoom);
		player = MockPlayer(kingdom);
		player2 = MockPlayer(kingdom);
		player3 = MockPlayer(kingdom);
		player4 = MockPlayer(kingdom);
		var players = new List<IPlayer> { player2.Object, player.Object, player3.Object, player4.Object };
		player.Setup(p => p.Game.Players).Returns(players);
	}

	[TestMethod]
	public void Play()
	{
		#region act
		councilRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Buy, +0 Actions, +0 Coins
		Assert.AreEqual(1, player.Object.PlayerState.Buys);
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);

		// +4 Cards
		player.Verify(p => p.Draw(4), Times.Once);

		// all the other players draw one card
		player2.Verify(p => p.Draw(1), Times.Once);
		player3.Verify(p => p.Draw(1), Times.Once);
		player4.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { councilRoom };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == councilRoom))).Returns(councilRoom);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// (+1 Buy, +0 Actions, +0 Buys) * 2  
		Assert.AreEqual(2, player.Object.PlayerState.Buys);
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// (+4 Cards) * 2
		player.Verify(p => p.Draw(4), Times.Exactly(2));

		// all the other players draw one card two times
		player2.Verify(p => p.Draw(1), Times.Exactly(2));
		player3.Verify(p => p.Draw(1), Times.Exactly(2));
		player4.Verify(p => p.Draw(1), Times.Exactly(2));
		#endregion
	}
}