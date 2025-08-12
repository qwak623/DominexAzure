using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class BureaucratTests : CardTestsBase
{
	private readonly Card bureaucrat = Bureaucrat.Get();
	private readonly Card province = Province.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(bureaucrat);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);

		var players = new List<IPlayer> { attacker.Object, defender.Object };
		attacker.Setup(a => a.Game.Players).Returns(players);
		defender.Setup(a => a.Game.Players).Returns(players);
	}

	[TestMethod]
	public void GainSilver()
	{
		#region act
		bureaucrat.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +0 Cards
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player gets a silver onto his draw pile
		attacker.Verify(p => p.GainToDrawPile(CardType.Silver), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.Object.PlayerState.Hand = new List<Card> { bureaucrat };
		attacker.Setup(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == bureaucrat))).Returns(bureaucrat);
		#endregion

		#region act
		throneRoom.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// (+0 Actions, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// +0 Cards
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player gets a silver onto his draw pile two times
		attacker.Verify(p => p.GainToDrawPile(CardType.Silver), Times.Exactly(2));

		// attacker was asked which card to play using throne room
		attacker.Verify(p => p.User.ThroneRoomPlay(throneRoom, attacker.Object.PlayerState,
			attacker.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// attacker deals an attack to the defender two times
		defender.Verify(d => d.DealAttack(attacker.Object, bureaucrat), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasVictory()
	{
		#region arrange
		defender.Object.PlayerState.Hand = new List<Card> { province, province };
		defender.Setup(d => d.User.BureaucratPutOnTop(bureaucrat, defender.Object.PlayerState, defender.Object.Game.Kingdom)).Returns(province);
		#endregion

		#region act
		bureaucrat.Attack(defender.Object, attacker.Object);
		#endregion

		#region assert
		// user is asked to choose a victory card to put on top
		defender.Verify(d => d.User.BureaucratPutOnTop(bureaucrat, defender.Object.PlayerState, defender.Object.Game.Kingdom), Times.Once);

		// defender puts the victory card on top
		defender.Verify(d => d.ReturnToDrawPile(province), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasNoVictoryCard()
	{
		#region arrange
		defender.Object.PlayerState.Hand = new List<Card> { };
		#endregion

		#region act
		bureaucrat.Attack(defender.Object, attacker: attacker.Object);
		#endregion

		#region assert
		// user is not asked to choose a victory card to put on top
		defender.Verify(d => d.User.BureaucratPutOnTop(bureaucrat, defender.Object.PlayerState, defender.Object.Game.Kingdom), Times.Never);

		// defender does not put any card on top
		defender.Verify(d => d.ReturnToDrawPile(It.IsAny<Card>()), Times.Never);
		#endregion
	}
}