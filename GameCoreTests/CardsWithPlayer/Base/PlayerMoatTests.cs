using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMoatTests : CardWithPlayerTestsBase
{
	private readonly Card moat = Moat.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card militia = Militia.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { moat, militia });

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void Play()
	{
		#region arrange
		defender.PlayerState.Hand = CreatePile([moat]);
		defender.PlayerState.DrawPile = CreatePile([copper, copper]);
		var moatToPlay = defender.PlayerState.Hand[0];
		#endregion

		#region act
		defender.PlayActionCardInternal(moatToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([moat], defender.PlayerState.CardsPlayed);
		AssertPile([moat], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		defender.PlayerState.Hand = CreatePile([throneRoom, moat]);
		defender.PlayerState.DrawPile = CreatePile([copper, copper, copper, copper]);
		var moatToPlay = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Moat);

		defenderUser.Setup(u => u.ThroneRoomPlay(throneRoom, defender.PlayerState,
			defender.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == moatToPlay))).Returns(moatToPlay);
		#endregion

		#region act
		defender.PlayActionCardInternal(defender.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper, copper, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([moat, throneRoom], defender.PlayerState.CardsPlayed);
		AssertPile([moat, moat, throneRoom], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.ThroneRoomPlay(throneRoom, defender.PlayerState,
			defender.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void React()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([copper, copper, moat, copper, copper]);
		var moatInHand = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Moat);

		defenderUser.Setup(u => u.PlayReactionCard(militia, defender.PlayerState,
			defender.Game.Kingdom, It.Is<List<CardInstance>>(r => r.Single() == moatInHand)))
			.Returns(moatInHand);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// reaction should not need or consume an action
		AssertNumbers(0, 0, 0, defender);

		// revealing a reaction doesn't move it out of hand or into played cards
		AssertPile([copper, copper, moat, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.PlayReactionCard(militia, defender.PlayerState,
			defender.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the moat blocked the attack, so militia's own effect never runs
		defenderUser.Verify(du => du.MilitiaDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontReact()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = CreatePile([copper, copper, moat, copper, copper]);
		var moatInHand = defender.PlayerState.Hand.First(c => c.Card.Name == CardName.Moat);
		var coppersToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Name == CardName.Copper).Take(2).ToList();

		defenderUser.Setup(u => u.PlayReactionCard(militia, defender.PlayerState,
			defender.Game.Kingdom, It.Is<List<CardInstance>>(r => r.Single() == moatInHand)))
			.Returns((CardInstance)null);
		defenderUser.Setup(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2))
			.Returns(coppersToDiscard);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, defender);

		// declining the reaction leaves moat in hand and lets the attack resolve normally
		AssertPile([moat, copper, copper], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.PlayReactionCard(militia, defender.PlayerState,
			defender.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		defenderUser.Verify(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, It.IsAny<List<CardInstance>>(), 2), Times.Once);
		#endregion
	}
}
