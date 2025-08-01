using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MilitiaTests : CardTestsBase
{
	private readonly Card militia = Militia.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(militia);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);
	}

	[TestMethod]
	public void Play()
	{
		#region act
		militia.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +2 Coins
		Assert.AreEqual(2, attacker.Object.PlayerState.Coins);

		// actions and buys shouldn't change
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player does not draw any cards
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod()]
	public void Attack_DiscardTwoCards()
	{
		#region arrange
		defender.Object.PlayerState.Hand = new List<Card> { militia, militia, militia, militia, militia };
		defender.Setup(d => d.User.MilitiaDiscard(militia, defender.Object.PlayerState, defender.Object.Game.Kingdom, 2))
			.Returns(new List<Card> { militia, militia });
		#endregion

		#region act
		militia.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// user is asked to choose two cards to discard
		defender.Verify(d => d.User.MilitiaDiscard(militia, defender.Object.PlayerState, defender.Object.Game.Kingdom, 2), Times.Once);

		// defender discards two militias
		defender.Verify(d => d.Discard(militia), Times.Exactly(2));
		#endregion
	}

	[TestMethod()]
	public void Attack_DontDiscardAnything()
	{
		#region arrange
		defender.Object.PlayerState.Hand = new List<Card> { militia, militia, militia };
		#endregion

		#region act
		militia.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// user is never asked to choose cards to discard
		defender.Verify(d => d.User.MilitiaDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);

		// defender discards nothing
		defender.Verify(d => d.Discard(It.IsAny<Card>()), Times.Never);
		#endregion
	}
}