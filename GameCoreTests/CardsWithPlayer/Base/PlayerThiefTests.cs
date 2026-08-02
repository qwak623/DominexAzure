using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerThiefTests : CardWithPlayerTestsBase
{
	private readonly Card thief = Thief.Get();
	private readonly Card province = Province.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(thief);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);
		defender.PlayerState.Actions = 0;

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void Attack_NoCardsToShow()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		// attacker did not steal anything
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender's actions, coins and buys shouldn't change
		// defender's hand did not change
		// defender has nothing to discard
		// nothing was trashed
		// nothing was added to the defender's played cards or actions
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([], defender.Game.Trash);

		// the attacker is not asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IEnumerable<CardInstance>>()), Times.Never);

		// the attacker is not asked whether to steal anything
		attackerUser.Verify(au => au.ThiefSteal(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<CardInstance>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void Attack_NoTreasuresToSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		defender.PlayerState.DrawPile = CreatePile([province, province]);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the shown cards
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([province, province], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([], defender.Game.Trash);

		// the attacker is not asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IEnumerable<CardInstance>>()), Times.Never);

		// the attacker is not asked whether to steal anything
		attackerUser.Verify(au => au.ThiefSteal(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<CardInstance>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void Attack_DontSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		defender.PlayerState.DrawPile = CreatePile([copper, province]);

		var copperToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Copper);
		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<CardInstance>>(c => c.Single() == copperToSteal))).Returns(copperToSteal);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom, copperToSteal)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the other card
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([province], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		AssertPile([copper], defender.Game.Trash);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			copperToSteal), Times.Once);
		#endregion
	}

	[TestMethod]
	public void Attack_Steal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		defender.PlayerState.DrawPile = CreatePile([province, gold]);

		var goldToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Gold);
		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<CardInstance>>(c => c.Single() == goldToSteal))).Returns(goldToSteal);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		// attacker gained the gold
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([gold], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the other card
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([province], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		// the gold was trashed, but the thief stole it
		AssertPile([], defender.Game.Trash);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal), Times.Once);
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresDontSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		defender.PlayerState.DrawPile = CreatePile([copper, silver]);

		var silverToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Silver);
		var copperToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Copper);
		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 2 && c.Contains(copperToSteal)
			&& c.Contains(silverToSteal)))).Returns(silverToSteal);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silverToSteal)).Returns(false);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		// attacker gained the gold
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the other card
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		// the silver is trashed
		AssertPile([silver], defender.Game.Trash);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silverToSteal), Times.Once);
		#endregion
	}

	[TestMethod]
	public void Attack_TwoTreasuresSteal()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([thief]);
		defender.PlayerState.DrawPile = CreatePile([gold, copper]);

		var goldToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Gold);
		var copperToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Copper);
		attackerUser.Setup(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<IEnumerable<CardInstance>>(c => c.Count() == 2 && c.Contains(copperToSteal)
			&& c.Contains(goldToSteal)))).Returns(goldToSteal);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal)).Returns(true);
		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand.First());
		#endregion

		#region assert
		// +0 Cards
		// attacker gained the gold
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([gold], attacker.PlayerState.DiscardPile);
		AssertPile([thief], attacker.PlayerState.CardsPlayed);
		AssertPile([thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the other card
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		// the gold was trashed, but the thief stole it
		AssertPile([], defender.Game.Trash);

		// the attacker is asked to choose a treasure to trash
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// the attacker is asked whether to steal the trashed card
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([throneRoom, thief]);
		defender.PlayerState.DrawPile = CreatePile([silver, copper, copper, gold]);

		var thiefToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Thief);
		attackerUser.Setup(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == thiefToPlay))).Returns(thiefToPlay);

		var goldToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Gold);
		var silverToSteal = defender.PlayerState.DrawPile.First(c => c.Card.Type == CardType.Silver);
		attackerUser.SetupSequence(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>())).Returns(goldToSteal).Returns(silverToSteal);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal)).Returns(true);
		attackerUser.Setup(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silverToSteal)).Returns(false);

		#endregion

		#region act
		attacker.PlayActionCardInternal(attacker.PlayerState.Hand[0]);
		#endregion

		#region assert
		// +0 Cards
		// thief and throne room were added to played cards
		// attacker gained the gold
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([gold], attacker.PlayerState.DiscardPile);
		AssertPile([throneRoom, thief], attacker.PlayerState.CardsPlayed);
		AssertPile([throneRoom, thief, thief], attacker.PlayerState.ActionsPlayed);

		// defender discards the other card
		AssertNumbers(0, 0, 0, defender);
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([copper, copper], defender.PlayerState.DiscardPile);
		AssertPile([], defender.PlayerState.CardsPlayed);
		AssertPile([], defender.PlayerState.ActionsPlayed);

		// silver and gold were trashed, but the thief stole the gold
		AssertPile([silver], defender.Game.Trash);

		// attacker was asked which card to play using throne room
		attackerUser.Verify(u => u.ThroneRoomPlay(throneRoom, attacker.PlayerState,
			attacker.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);

		// the attacker is asked to choose a treasure to trash two times
		attackerUser.Verify(au => au.ThiefChoose(thief, attacker.PlayerState, attacker.Game.Kingdom,
			It.IsAny<IEnumerable<CardInstance>>()), Times.Exactly(2));

		// the attacker is asked whether to steal the trashed card two times
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			goldToSteal), Times.Once);
		attackerUser.Verify(au => au.ThiefSteal(thief, attacker.PlayerState, attacker.Game.Kingdom,
			silverToSteal), Times.Once);
		#endregion
	}
}