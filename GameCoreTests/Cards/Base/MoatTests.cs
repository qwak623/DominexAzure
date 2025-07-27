using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MoatTests : CardTestsBase
{
	private readonly Card moat = Moat.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(moat));
	}

	[TestMethod]
	public void Reaction_Defend()
	{
		#region arrange
		player.Setup(p => p.User.MoatDefend(moat, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(true);
		#endregion

		#region act
		bool defended = moat.Reaction(player.Object);
		#endregion

		#region assert
		Assert.AreEqual(true, defended);
		#endregion
	}

	[TestMethod]
	public void Reaction_DontDefend()
	{
		#region arrange
		player.Setup(p => p.User.MoatDefend(moat, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(false);
		#endregion

		#region act
		bool defended = moat.Reaction(player.Object);
		#endregion

		#region assert
		Assert.AreEqual(false, defended);
		#endregion
	}
}