using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MilitiaTests : CardTestsBase
{
	private readonly Card militia = Militia.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(militia);
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
		militia.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +0 Actions, +2 Coins, +0 Buys
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(2, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +0 Cards
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.Object.PlayerState.Hand = new List<Card> { militia };
		attacker.Setup(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == militia))).Returns(militia);
		#endregion

		#region act
		throneRoom.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// (+0 Actions, +2 Coins, +0 Buys) * 2
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(4, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +0 Cards
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// attacker was asked which card to play using throne room
		attacker.Verify(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// attacker deals an attack to the defender two times
		defender.Verify(d => d.DealAttack(attacker.Object, militia), Times.Exactly(2));
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