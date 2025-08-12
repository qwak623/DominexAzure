using System.Numerics;
using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerSpyTests : CardWithPlayerTestsBase
{
	private readonly Card spy = Spy.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card province = Province.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player1;
	private Player player2;
	private Player player3;
	private Player player4;

	private Mock<IUser> user1;
	private Mock<IUser> user2;
	private Mock<IUser> user3;
	private Mock<IUser> user4;

	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(spy);

		user1 = new Mock<IUser>();
		user2 = new Mock<IUser>();
		user3 = new Mock<IUser>();
		user4 = new Mock<IUser>();

		player1 = CreatePlayer(game.Object, user1.Object);
		player2 = CreatePlayer(game.Object, user2.Object);
		player3 = CreatePlayer(game.Object, user3.Object);
		player4 = CreatePlayer(game.Object, user4.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { player2, player1, player3, player4 });
	}

	[TestMethod]
	public void PlayerDiscardsCard()
	{
		#region arrange
		player1.PlayerState.Hand = new List<Card> { spy };
		player1.PlayerState.DrawPile = new List<Card> { province, duchy };

		player2.PlayerState.DrawPile = new List<Card> { province };

		player3.PlayerState.DrawPile = new List<Card> { province };

		// TODO až bude jasné, komu odhazujeme kartu, chtělo by to tady udělat líp
		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, province, Phase.Action))
			.Returns(true);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, province, Phase.Attack))
			.Returns(true).Returns(false);
		#endregion

		#region act
		player1.PlayActionCardInternal(spy);
		#endregion

		#region assert
		// (-1 Action, +1 Action), +0 Coins, +0 Buys
		Assert.AreEqual(1, player1.PlayerState.Actions);
		Assert.AreEqual(0, player1.PlayerState.Coins);
		Assert.AreEqual(0, player1.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { duchy }, player1.PlayerState.Hand);

		// user is asked whether to discard his card in the action phase
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			province, Phase.Action), Times.Once);

		// the player1's card is added to the discard pile, not to the draw pile
		Assert.IsFalse(player1.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { province }, player1.PlayerState.DiscardPile);

		// user is asked whether to discard the two other players' cards in the attack phase
		// (the last player does not have any card to show)
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			province, Phase.Attack), Times.Exactly(2));

		// the player2's card is added to the discard pile, not to the draw pile
		Assert.IsFalse(player2.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { province }, player2.PlayerState.DiscardPile);

		// the player3's card is added to the draw pile, not to the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, player3.PlayerState.DrawPile);
		Assert.IsFalse(player3.PlayerState.DiscardPile.Any());

		// player4 did not have any card to show
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		Assert.IsFalse(player4.PlayerState.DiscardPile.Any());

		// spy was added to the player1's played cards
		CollectionAssert.AreEquivalent(new List<Card> { spy }, player1.PlayerState.PlayedCards);

		// spy was not added to the other players' played cards
		Assert.IsFalse(player2.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player3.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player4.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	public void PlayerDoesntDiscardCard()
	{
		#region arrange
		player1.PlayerState.Hand = new List<Card> { spy };
		player1.PlayerState.DrawPile = new List<Card> { silver, duchy };

		player2.PlayerState.DrawPile = new List<Card> { province };

		player4.PlayerState.DrawPile = new List<Card> { province };

		// TODO až bude jasné, komu odhazujeme kartu, chtělo by to tady udělat líp
		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, silver, Phase.Action))
			.Returns(false);

		user1.SetupSequence(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, province, Phase.Attack))
			.Returns(false).Returns(true);
		#endregion

		#region act
		player1.PlayActionCardInternal(spy);
		#endregion

		#region assert
		// (-1 Action, +1 Action), +0 Coins, +0 Buys
		Assert.AreEqual(1, player1.PlayerState.Actions);
		Assert.AreEqual(0, player1.PlayerState.Coins);
		Assert.AreEqual(0, player1.PlayerState.Buys);

		// +1 Card
		CollectionAssert.AreEquivalent(new List<Card> { duchy }, player1.PlayerState.Hand);

		// user is asked whether to discard his card in the action phase
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			silver, Phase.Action), Times.Once);

		// the player1's card is added to the discard pile, not to the draw pile
		CollectionAssert.AreEquivalent(new List<Card> { silver }, player1.PlayerState.DrawPile);
		Assert.IsFalse(player1.PlayerState.DiscardPile.Any());

		// user is asked whether to discard the two other players' cards in the attack phase
		// (the last player does not have any card to show)
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			province, Phase.Attack), Times.Exactly(2));

		// the player2's card is added to the draw pile, not to the discard pile
		CollectionAssert.AreEquivalent(new List<Card> { province }, player2.PlayerState.DrawPile);
		Assert.IsFalse(player2.PlayerState.DiscardPile.Any());

		// player3 did not have any card to show
		Assert.IsFalse(player3.PlayerState.DrawPile.Any());
		Assert.IsFalse(player3.PlayerState.DiscardPile.Any());

		// the player4's card is added to the discard pile, not to the draw pile
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		CollectionAssert.AreEquivalent(new List<Card> { province }, player4.PlayerState.DiscardPile);

		// spy was added to the player1's played cards
		CollectionAssert.AreEquivalent(new List<Card> { spy }, player1.PlayerState.PlayedCards);

		// spy was not added to the other players' played cards
		Assert.IsFalse(player2.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player3.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player4.PlayerState.PlayedCards.Any());
		#endregion
	}

	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public void ThroneRoomPlay(bool discardFirstTime, bool discardSecondTime)
	{
		#region arrange
		player1.PlayerState.Hand = new List<Card> { throneRoom, spy };
		player1.PlayerState.DrawPile = new List<Card> { province, silver, spy, duchy };
		player2.PlayerState.DrawPile = new List<Card> { province, silver };
		player3.PlayerState.DrawPile = new List<Card> { province };
		player4.PlayerState.DrawPile = new List<Card> { };

		user1.Setup(u => u.ThroneRoomPlay(throneRoom, player1.PlayerState,
			player1.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == spy))).Returns(spy);

		user1.SetupSequence(du => du.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, It.IsAny<Card>(), Phase.Action))
			.Returns(discardFirstTime).Returns(discardSecondTime);
		user1.SetupSequence(du => du.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom, It.IsAny<Card>(), Phase.Attack))
			.Returns(discardFirstTime).Returns(discardFirstTime).Returns(discardSecondTime).Returns(discardSecondTime);
		#endregion

		#region act
		player1.PlayActionCardInternal(throneRoom);
		#endregion

		#region assert
		// -1 Action, (+1 Action, +0 Coins, +0 Buys) * 2
		Assert.AreEqual(2, player1.PlayerState.Actions);
		Assert.AreEqual(0, player1.PlayerState.Coins);
		Assert.AreEqual(0, player1.PlayerState.Buys);

		// user is asked whether to discard his card in the action phase twice
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.IsAny<Card>(), Phase.Action), Times.Exactly(2));

		// user is asked whether to discard the other players' cards in the attack phase
		user1.Verify(au => au.SpyDiscard(spy, player1.PlayerState, player1.Game.Kingdom,
			It.IsAny<Card>(), Phase.Attack), Times.Exactly(4));

		// assert all player1, player2 and player3's cards
		switch ((discardFirstTime, discardSecondTime))
		{
			case (true, true):
				CollectionAssert.AreEquivalent(new List<Card> { }, player1.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { duchy, silver }, player1.PlayerState.Hand);
				CollectionAssert.AreEquivalent(new List<Card> { spy, province }, player1.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { }, player2.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { silver, province }, player2.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { }, player3.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { province }, player3.PlayerState.DiscardPile);
				break;
			case (true, false):
				CollectionAssert.AreEquivalent(new List<Card> { province }, player1.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { duchy, silver }, player1.PlayerState.Hand);
				CollectionAssert.AreEquivalent(new List<Card> { spy }, player1.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { province }, player2.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { silver }, player2.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { province }, player3.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { }, player3.PlayerState.DiscardPile);
				break;
			case (false, true):
				CollectionAssert.AreEquivalent(new List<Card> { province }, player1.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { duchy, spy }, player1.PlayerState.Hand);
				CollectionAssert.AreEquivalent(new List<Card> { silver }, player1.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { province }, player2.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { silver }, player2.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { }, player3.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { province }, player3.PlayerState.DiscardPile);
				break;
			case (false, false):
				CollectionAssert.AreEquivalent(new List<Card> { province, silver }, player1.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { duchy, spy }, player1.PlayerState.Hand);
				CollectionAssert.AreEquivalent(new List<Card> { }, player1.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { province, silver }, player2.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { }, player2.PlayerState.DiscardPile);

				CollectionAssert.AreEquivalent(new List<Card> { province }, player3.PlayerState.DrawPile);
				CollectionAssert.AreEquivalent(new List<Card> { }, player3.PlayerState.DiscardPile);
				break;
		}

		// all the other players' hands did not change
		CollectionAssert.AreEquivalent(new List<Card> { }, player2.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { }, player3.PlayerState.Hand);
		CollectionAssert.AreEquivalent(new List<Card> { }, player4.PlayerState.Hand);

		// player4 did not have any card to show
		Assert.IsFalse(player4.PlayerState.DrawPile.Any());
		Assert.IsFalse(player4.PlayerState.DiscardPile.Any());

		// spy and throne room were added to the player1's played cards
		CollectionAssert.AreEquivalent(new List<Card> { spy, throneRoom }, player1.PlayerState.PlayedCards);

		// nothing was added to the other players' played cards
		Assert.IsFalse(player2.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player3.PlayerState.PlayedCards.Any());
		Assert.IsFalse(player4.PlayerState.PlayedCards.Any());
		#endregion
	}
}