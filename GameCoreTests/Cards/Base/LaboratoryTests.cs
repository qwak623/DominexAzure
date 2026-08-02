#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class LaboratoryTests : CardTestsBase
{
	private readonly Card laboratory = Laboratory.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(laboratory));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		laboratory.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +1 Action, +0 Coins, +0 Buys
		Assert.AreEqual(1, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +2 Cards
		player.Verify(p => p.Draw(2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { laboratory };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == laboratory))).Returns(laboratory);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// (+1 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// (+2 Cards) * 2
		player.Verify(p => p.Draw(2), Times.Exactly(2));

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);
		#endregion
	}
}
#endif
