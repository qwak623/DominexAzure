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

	private Mock<IPlayer> attacker;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		var kingdom = MockKingdom(bureaucrat);
		attacker = MockPlayer(kingdom);
		defender = MockPlayer(kingdom);
	}

	[TestMethod]
	public void GainSilver()
	{
		#region act
		bureaucrat.WhenPlayAction(attacker.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, attacker.Object.PlayerState.Actions);
		Assert.AreEqual(0, attacker.Object.PlayerState.Coins);
		Assert.AreEqual(0, attacker.Object.PlayerState.Buys);

		// player does not draw a card
		attacker.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player gets a silver onto his draw pile
		attacker.Verify(p => p.GainToDrawPile(CardType.Silver), Times.Once);
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