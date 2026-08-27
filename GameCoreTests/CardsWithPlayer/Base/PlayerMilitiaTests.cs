using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMilitiaTests : CardWithPlayerTestsBase
{
	private readonly Card militia = Militia.Get();
	private readonly Card copper = Copper.Get();
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
		game = MockGame(militia);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void Attack_DiscardTwoCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([militia]);
		var militiaToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.Hand = CreatePile([silver, silver, silver, silver, copper]);
		var silverToDiscard = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);
		var copperToDiscard = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		defenderUser.Setup(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2))
			.Returns([silverToDiscard, copperToDiscard]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(militiaToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([militia], attacker.PlayerState.CardsPlayed);
		AssertPile([militia], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// attacking doesn't touch the attacker's own actions/coins/buys beyond militia's own stats
		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, silver, silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([silver, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void Attack_DontDiscardAnything()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([militia]);
		var militiaToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.Hand = CreatePile([copper, copper, silver]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(militiaToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([militia], attacker.PlayerState.CardsPlayed);
		AssertPile([militia], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		// hand is already at 3 or fewer, so the defender is never asked to discard
		defenderUser.Verify(du => du.MilitiaDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomDiscardTwoCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, militia]);
		var militiaToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Militia);
		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == militiaToPlay))).Returns(militiaToPlay);

		defender.PlayerState.Hand = CreatePile([silver, silver, silver, silver, copper]);
		var silverToDiscard = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);
		var copperToDiscard = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		defenderUser.Setup(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2))
			.Returns([silverToDiscard, copperToDiscard]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 4, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([militia, throneRoom], attacker.PlayerState.CardsPlayed);
		AssertPile([militia, militia, throneRoom], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver, silver, silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([silver, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the attack fires once per throne-room resolution, but the defender's hand only drops
		// to 3 after the first one, so the second attack's <=3 guard makes it a no-op
		defenderUser.Verify(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Once);
		#endregion
	}
}
