#if false
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class WitchTests : CardTestsBase
{
	private readonly Card witch = Witch.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(witch);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);

		var players = new List<IPlayer> { attacker.Object, defender.Object };
		attacker.Setup(a => a.Game.Players).Returns(players);
		defender.Setup(a => a.Game.Players).Returns(players);
	}

	[TestMethod]
	public void Play()
	{
		#region act
		witch.WhenPlayAction(attacker.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +2 Cards
		attacker.Verify(p => p.Draw(2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.Object.PlayerState.Hand = new List<Card> { witch };
		attacker.Setup(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == witch))).Returns(witch);
		#endregion

		#region act
		throneRoom.WhenPlayAction(attacker.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// (+2 Cards) * 2
		attacker.Verify(p => p.Draw(2), Times.Exactly(2));

		// attacker was asked which card to play using throne room
		attacker.Verify(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// attacker deals an attack to the defender two times
		defender.Verify(d => d.DealAttack(attacker.Object, witch), Times.Exactly(2));
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
#endif
