using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class GardensTests : CardTestsBase
{
	private readonly Card gardens = Gardens.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(gardens));
	}

	[TestMethod]
	public void CountPointsOnePoint()
	{
		#region arrange
		player.Setup(p => p.CardCount).Returns(13);
		#endregion

		#region act
		var points = gardens.CountPoints(player.Object);
		#endregion

		#region assert
		Assert.AreEqual(1, points);
		#endregion
	}
}