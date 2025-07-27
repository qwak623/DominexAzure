using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class FestivalTests : CardTestsBase
{
	private readonly Card festival = Festival.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(festival));
	}

	[TestMethod]
	public void Play()
	{
		#region act
		festival.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(2, player.Object.PlayerState.Actions);
		Assert.AreEqual(2, player.Object.PlayerState.Coins);
		Assert.AreEqual(1, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}
}