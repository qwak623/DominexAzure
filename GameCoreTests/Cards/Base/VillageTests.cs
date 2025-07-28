using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class VillageTests : CardTestsBase
{
	private readonly Card village = Village.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(village));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		village.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +2 Actions
		Assert.AreEqual(2, player.Object.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player draws one card
		player.Verify(p => p.Draw(1), Times.Once);
		#endregion
	}
}