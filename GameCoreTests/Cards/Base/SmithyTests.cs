using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class SmithyTests : CardTestsBase
{
	private readonly Card smithy = Smithy.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(smithy));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		smithy.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player draws three cards
		player.Verify(p => p.Draw(3), Times.Once);
		#endregion
	}
}