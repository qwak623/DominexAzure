using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class BureaucratTests
{
	private readonly Card bureaucrat = Bureaucrat.Get();
	private readonly Card province = Province.Get();

	private Kingdom kingdom;
	private PlayerState playerState;
	private PlayerState defenderPlayerState;

	private Mock<IPlayer> player;
	private Mock<IPlayer> defender;

	[TestInitialize]
	public void Init()
	{
		player = new Mock<IPlayer>();

		playerState = new PlayerState(playerStateObserver: null, "Tester")
		{
			Actions = 0,
			Coins = 0,
			Buys = 0,
			Hand = new List<Card> { },
		};
		player.Setup(p => p.PlayerState).Returns(playerState);

		defender = new Mock<IPlayer>();

		defenderPlayerState = new PlayerState(playerStateObserver: null, "Defender")
		{
			Hand = new List<Card> { },
		};
		defender.Setup(d => d.PlayerState).Returns(defenderPlayerState);

		kingdom = new Kingdom(new List<Card> { bureaucrat }, 2); // todo should be mockable
		player.Setup(p => p.Game.Kingdom).Returns(kingdom);
		defender.Setup(p => p.Game.Kingdom).Returns(kingdom);
	}

	[TestMethod]
	public void GainSilver()
	{
		#region act
		bureaucrat.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player gets a silver onto his draw pile
		player.Verify(p => p.GainToDrawPile(CardType.Silver), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasVictory()
	{
		#region arrange
		defenderPlayerState.Hand = new List<Card> { province, province };
		defender.Setup(d => d.User.BureaucratPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>())).Returns(province);
		#endregion

		#region act
		bureaucrat.Attack(defender.Object, attacker: player.Object);
		#endregion

		#region assert
		// user is asked to choose a victory card to put on top
		defender.Verify(d => d.User.BureaucratPutOnTop(bureaucrat, defender.Object.PlayerState, kingdom), Times.Once);

		// defender puts the victory card on top
		defender.Verify(d => d.ReturnToDrawPile(province), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasNoVictoryCard()
	{
		#region arrange
		defenderPlayerState.Hand = new List<Card> { };
		#endregion

		#region act
		bureaucrat.Attack(defender.Object, attacker: player.Object);
		#endregion

		#region assert
		// user is not asked to choose a victory card to put on top
		defender.Verify(d => d.User.BureaucratPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);

		// defender does not put any card on top
		defender.Verify(d => d.ReturnToDrawPile(It.IsAny<Card>()), Times.Never);
		#endregion
	}
}