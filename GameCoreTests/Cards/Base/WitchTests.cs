using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class WitchTests : CardTestsBase
{
	private readonly Card witch = Witch.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(witch);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);
	}

	[TestMethod]
	public void Play()
	{
		#region act
		witch.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player draws two cards
		attacker.Verify(p => p.Draw(2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void Attack()
	{
		#region act
		witch.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// defender gains the curse
		defender.Verify(d => d.Gain(CardType.Curse), Times.Once);
		#endregion
	}
}