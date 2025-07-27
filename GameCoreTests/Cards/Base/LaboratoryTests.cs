using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class LaboratoryTests : CardTestsBase
{
	private readonly Card laboratory = Laboratory.Get();

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
		laboratory.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +1 Action
		Assert.AreEqual(1, player.Object.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player draws two cards
		player.Verify(p => p.Draw(2), Times.Once);
		#endregion
	}
}