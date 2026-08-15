using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerWitchTests : CardWithPlayerTestsBase
{
	private readonly Card witch = Witch.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card curse = Curse.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(witch);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([witch]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver]);
		var witchToPlay = attacker.PlayerState.Hand[0];
		#endregion

		#region act
		attacker.PlayActionCardInternal(witchToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([witch], attacker.PlayerState.CardsPlayed);
		AssertPile([witch], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// defender got a curse to his discard pile; nothing else about him changed
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([curse], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, witch]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, curse, silver]);
		var witchToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Witch);

		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == witchToPlay))).Returns(witchToPlay);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, curse, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([witch, throneRoom], attacker.PlayerState.CardsPlayed);
		AssertPile([witch, witch, throneRoom], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the attack fires once per throne-room resolution, so the defender gets two curses
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([curse, curse], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);
		#endregion
	}
}
