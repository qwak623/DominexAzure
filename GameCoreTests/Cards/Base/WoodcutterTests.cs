using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class WoodcutterTests : CardTestsBase
{
	private readonly Card woodcutter = Woodcutter.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(woodcutter));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		woodcutter.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, player.Object.PlayerState.Coins);

		// +1 Buy
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// Actions shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Actions);

		// player draws no cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}
}