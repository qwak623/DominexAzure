using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerTorturerTests : CardWithPlayerTestsBase
{
	private readonly Card torturer = Torturer.Get();
	private readonly Card copper = Copper.Get();
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
		game = MockGame(torturer);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns([attacker, defender]);
	}

	[TestMethod]
	public void DefenderChoosesCurse()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([torturer]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver]);
		var torturerToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.Hand = CreatePile([copper, copper]);

		defenderUser.Setup(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(torturerToPlay);
		#endregion

		#region assert
		// attacker draws 3, its own bonus
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([torturer], attacker.PlayerState.CardsPlayed);
		AssertPile([torturer], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// the curse goes straight into the defender's hand, not the discard pile
		AssertNumbers(0, 0, 0, defender);
		AssertPile([copper, copper, curse], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom), Times.Once);
		defenderUser.Verify(u => u.TorturerDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DefenderChoosesDiscardWithTwoOrMoreCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([torturer]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver]);
		var torturerToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.Hand = CreatePile([copper, copper, silver]);
		var coppersToDiscard = defender.PlayerState.Hand.Where(c => c.Card.Type == CardType.Copper).ToList();

		defenderUser.Setup(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom)).Returns(false);
		defenderUser.Setup(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 2))
			.Returns(coppersToDiscard);
		#endregion

		#region act
		attacker.PlayActionCardInternal(torturerToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([torturer], attacker.PlayerState.CardsPlayed);
		AssertPile([torturer], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([silver], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		defenderUser.Verify(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom), Times.Once);
		// asked to discard exactly 2, not the whole (bigger) hand
		defenderUser.Verify(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 2), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DefenderChoosesDiscardWithFewerThanTwoCards()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([torturer]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver]);
		var torturerToPlay = attacker.PlayerState.Hand[0];

		defender.PlayerState.Hand = CreatePile([copper]);
		var copperToDiscard = defender.PlayerState.Hand.ToList();

		defenderUser.Setup(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom)).Returns(false);
		defenderUser.Setup(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 1))
			.Returns(copperToDiscard);
		#endregion

		#region act
		attacker.PlayActionCardInternal(torturerToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([torturer], attacker.PlayerState.CardsPlayed);
		AssertPile([torturer], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		// asked to discard only 1 - the defender doesn't have two cards to give up
		defenderUser.Verify(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 1), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DefenderChoosesDiscardWithEmptyHand()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([torturer]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver]);
		var torturerToPlay = attacker.PlayerState.Hand[0];

		// defender.PlayerState.Hand stays empty (CardWithPlayerTestsBase default)
		defenderUser.Setup(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(torturerToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([torturer], attacker.PlayerState.CardsPlayed);
		AssertPile([torturer], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		// nothing to discard, so the defender is never asked how many
		defenderUser.Verify(u => u.TorturerDiscard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<int>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomRecalculatesDiscardCountBetweenResolutions()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, torturer]);
		attacker.PlayerState.DrawPile = CreatePile([silver, silver, silver, silver, silver, silver]);
		var torturerToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Torturer);

		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == torturerToPlay))).Returns(torturerToPlay);

		defender.PlayerState.Hand = CreatePile([copper, copper, copper]);
		var coppersInHand = defender.PlayerState.Hand.ToList();

		defenderUser.Setup(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom)).Returns(false);
		defenderUser.Setup(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 2))
			.Returns([coppersInHand[0], coppersInHand[1]]);
		defenderUser.Setup(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 1))
			.Returns([coppersInHand[2]]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([silver, silver, silver, silver, silver, silver], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([torturer, throneRoom], attacker.PlayerState.CardsPlayed);
		AssertPile([torturer, torturer, throneRoom], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		// first resolution asks for 2, leaving the defender with 1 card; the second
		// resolution correctly recalculates down to 1 instead of asking for 2 again
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);
		AssertPile([], defender.Game.Trash);

		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		defenderUser.Verify(u => u.TorturerChooseCurse(torturer, defender.PlayerState, defender.Game.Kingdom), Times.Exactly(2));
		defenderUser.Verify(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 2), Times.Once);
		defenderUser.Verify(u => u.TorturerDiscard(torturer, defender.PlayerState, defender.Game.Kingdom, 1), Times.Once);
		#endregion
	}
}
