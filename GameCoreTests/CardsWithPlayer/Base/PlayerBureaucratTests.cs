using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerBureaucratTests : CardWithPlayerTestsBase
{
	private readonly Card bureaucrat = Bureaucrat.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card province = Province.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(bureaucrat);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void AttackPlayerThatHasVictory()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bureaucrat]);
		defender.PlayerState.Hand = CreatePile([province, duchy]);

		var provinceInHand = defender.PlayerState.Hand.First(c => c.Card.Type == CardType.Province);
		defenderUser.Setup(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom)).Returns(provinceInHand);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand[0]);
		#endregion

		#region assert
		// attacker got silver to his draw pile
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([silver], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([bureaucrat], attacker.PlayerState.CardsPlayed);
		AssertPile([bureaucrat], attacker.PlayerState.ActionsPlayed);

		// province was put on defender's draw pile
		// the other card stayed in the defender's hand
		AssertNumbers(0, 0, 0, defender);
		AssertPile([duchy], defender.PlayerState.Hand);
		AssertPile([province], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([], defender.Game.Trash);

		// defender's user is asked to choose a victory card to put on top
		defenderUser.Verify(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AttackPlayerThatHasNoVictoryCard()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([bureaucrat]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand[0]);
		#endregion

		#region assert
		// attacker got silver to his draw pile
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([silver], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([bureaucrat], attacker.PlayerState.CardsPlayed);
		AssertPile([bureaucrat], attacker.PlayerState.ActionsPlayed);

		// province was put on defender's draw pile
		// the other card stayed in the defender's hand
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([], defender.Game.Trash);

		// defender's user is not asked to choose a victory card to put on top
		defenderUser.Verify(du => du.BureaucratPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>()), Times.Never);
		#endregion
	}

	[TestMethod]
	[DataRow(0, 0)]
	[DataRow(1, 1)]
	[DataRow(2, 2)]
	[DataRow(3, 2)]
	public void ThroneRoomPlay(int provinceCount, int provincesPutOnTop)
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, bureaucrat]);
		var bureaucratToPlay = attacker.PlayerState.Hand[1];
		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == bureaucratToPlay))).Returns(bureaucratToPlay);

		defender.PlayerState.Hand = CreatePile([.. Enumerable.Repeat(province, provinceCount), silver, bureaucrat]);
		var provincesInHand = defender.PlayerState.Hand.Where(c => c.Card.Type == CardType.Province).ToList();
		var firstProvince = provincesInHand.FirstOrDefault();
		if (firstProvince != null)
		{
			defenderUser.SetupSequence(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom))
				.Returns(firstProvince).Returns(provincesInHand.Last());
		}
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand[0]);
		#endregion

		#region assert
		// attacker got two silvers to his draw pile
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([silver, silver], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([throneRoom, bureaucrat], attacker.PlayerState.CardsPlayed);
		AssertPile([throneRoom, bureaucrat, bureaucrat], attacker.PlayerState.ActionsPlayed);

		// if there were up to two provinces in defender's hand, they are now at his draw pile
		// non-victory cards stayed in defender's hand
		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, bureaucrat, .. Enumerable.Repeat(province, provinceCount - provincesPutOnTop)], defender.PlayerState.Hand);
		AssertPile([.. Enumerable.Repeat(province, provincesPutOnTop)], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([], defender.Game.Trash);

		// attacker was asked which card to play using throne room
		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// defender's user is asked to choose a victory card to put on top given amount of times
		defenderUser.Verify(du => du.BureaucratPutOnTop(bureaucrat, defender.PlayerState, defender.Game.Kingdom), Times.Exactly(provincesPutOnTop));
		#endregion
	}
}