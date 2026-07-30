using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		defender.PlayerState.Hand = new List<Card> { moat };
		defender.PlayerState.DrawPile = new List<Card> { copper, copper };
		#endregion

		#region act
		defender.PlayActionCardInternal(moat);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, defender.PlayerState.Actions);

		// +0 Coins, +0 Buys
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// +2 Cards
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, defender.PlayerState.Hand);
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());

		// moat was added to played cards and actions
		CollectionAssert.AreEquivalent(new List<Card> { moat }, defender.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { moat }, defender.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		defender.PlayerState.Hand = new List<Card> { throneRoom, moat };
		defender.PlayerState.DrawPile = new List<Card> { copper, copper, copper, copper };
		defenderUser.Setup(u => u.ThroneRoomPlay(throneRoom, defender.PlayerState,
			defender.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == moat))).Returns(moat);

		#endregion
		#region act
		defender.PlayActionCardInternal(throneRoom);

		#endregion
		#region assert
		// -1 Action, (+0 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(0, defender.PlayerState.Actions);
		Assert.AreEqual(0, defender.PlayerState.Coins);
		Assert.AreEqual(0, defender.PlayerState.Buys);

		// (+2 Card) * 2
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, copper, copper }, defender.PlayerState.Hand);
		Assert.IsFalse(defender.PlayerState.DrawPile.Any());
		Assert.IsFalse(defender.PlayerState.DiscardPile.Any());

		// defender was asked which card to play using throne room
		defenderUser.Verify(u => u.ThroneRoomPlay(throneRoom, defender.PlayerState,
			defender.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// moat and throne room were added to played cards
		CollectionAssert.AreEquivalent(new List<Card> { moat, throneRoom }, defender.PlayerState.CardsPlayed);

		// two moats and throne room were added to played actions
		CollectionAssert.AreEquivalent(new List<Card> { moat, moat, throneRoom }, defender.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void React()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = new List<Card> { copper, copper, moat, copper, copper };

		defenderUser.Setup(u => u.PlayCard(It.Is<LinkedList<Card>>(r => r.SingleOrDefault() == moat),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns(moat);
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		Assert.AreEqual(0, defender.PlayerState.Actions);

		// user is asked to choose a reaction to play
		defenderUser.Verify(u => u.PlayCard(It.IsAny<LinkedList<Card>>(),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia), Times.Once);

		// player blocked the attack and therefore his hand did not change
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper, moat, copper, copper }, defender.PlayerState.Hand);

		// moat was not added to played cards or actions
		CollectionAssert.AreEquivalent(new List<Card> { }, defender.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, defender.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void DontReact()
	{
		#region arrange
		defender.PlayerState.Actions = 0;
		defender.PlayerState.Hand = new List<Card> { copper, copper, moat, copper, copper };

		defenderUser.Setup(u => u.PlayCard(It.Is<LinkedList<Card>>(r => r.SingleOrDefault() == moat),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia))
			.Returns<Card>(null);
		defenderUser.Setup(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 2))
			.Returns(new List<Card> { copper, copper });
		#endregion

		#region act
		defender.DealAttack(attacker, militia);
		#endregion

		#region assert
		// reaction should not need or consume action
		Assert.AreEqual(0, defender.PlayerState.Actions);

		// user is asked to choose a reaction to play
		defenderUser.Verify(u => u.PlayCard(It.IsAny<LinkedList<Card>>(),
			defender.PlayerState, defender.Game.Kingdom, Phase.Reaction, militia), Times.Once);

		// user chose not to block the attack and therefore he needs to choose two cards to discard
		defenderUser.Verify(du => du.MilitiaDiscard(militia, defender.PlayerState, defender.Game.Kingdom, 2), Times.Once);

		// player discarded two cards
		CollectionAssert.AreEquivalent(new List<Card> { moat, copper, copper }, defender.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { copper, copper }, defender.PlayerState.DiscardPile);

		// moat was not added to played cards or actions
		CollectionAssert.AreEquivalent(new List<Card> { }, defender.PlayerState.CardsPlayed);
		CollectionAssert.AreEquivalent(new List<Card> { }, defender.PlayerState.ActionsPlayed);
		#endregion
	}
}