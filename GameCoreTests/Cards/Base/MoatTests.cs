using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MoatTests : CardTestsBase
{
	// reaction should be tested in player

	private readonly Card moat = Moat.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(moat));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		moat.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player draws two cards
		player.Verify(p => p.Draw(2), Times.Once);
		#endregion
	}
}