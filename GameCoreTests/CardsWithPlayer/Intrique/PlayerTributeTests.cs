using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerTributeTests : CardWithPlayerTestsBase
{
	private readonly Card tribute = Tribute.Get();
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card harem = Harem.Get();

	private Player player;
	private Player neighbor;

	private Mock<IUser> user;
	private Mock<IUser> neighborUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(tribute);
		user = new Mock<IUser>();
		neighborUser = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
		neighbor = CreatePlayer(game.Object, neighborUser.Object);
		game.Setup(g => g.Players).Returns([player, neighbor]);
	}

	[TestMethod]
	public void AppliesBonusForEachDifferentlyNamedCard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tribute]);
		var tributeToPlay = player.PlayerState.Hand[0];

		neighbor.PlayerState.DrawPile = CreatePile([silver, village]);
		#endregion

		#region act
		player.PlayActionCardInternal(tributeToPlay);
		#endregion

		#region assert
		// village (action) grants +2 actions, silver (treasure) grants +2 coins
		AssertNumbers(2, 2, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tribute], player.PlayerState.CardsPlayed);
		AssertPile([tribute], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([], neighbor.PlayerState.DrawPile);
		AssertPile([village, silver], neighbor.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void SameNamedCardsOnlyCountOnce()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tribute]);
		var tributeToPlay = player.PlayerState.Hand[0];

		neighbor.PlayerState.DrawPile = CreatePile([copper, copper]);
		#endregion

		#region act
		player.PlayActionCardInternal(tributeToPlay);
		#endregion

		#region assert
		// both revealed cards are coppers - the treasure bonus only applies once
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tribute], player.PlayerState.CardsPlayed);
		AssertPile([tribute], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// both copies are still discarded even though only one of them counted for the bonus
		AssertPile([], neighbor.PlayerState.DrawPile);
		AssertPile([copper, copper], neighbor.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void DualTypeCardTriggersBothBonuses()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tribute]);
		player.PlayerState.DrawPile = CreatePile([copper, copper]);
		var tributeToPlay = player.PlayerState.Hand[0];

		// harem is the neighbor's only card, so it's the only one revealed
		neighbor.PlayerState.DrawPile = CreatePile([harem]);
		#endregion

		#region act
		player.PlayActionCardInternal(tributeToPlay);
		#endregion

		#region assert
		// harem is both a treasure and a victory card, so both +2 coins and +2 cards fire
		AssertNumbers(0, 2, 0, player);
		AssertPile([copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tribute], player.PlayerState.CardsPlayed);
		AssertPile([tribute], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([], neighbor.PlayerState.DrawPile);
		AssertPile([harem], neighbor.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void RevealsFewerThanTwoWhenNeighborHasFewerCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([tribute]);
		var tributeToPlay = player.PlayerState.Hand[0];

		// neighbor only has one card total, nothing left to reshuffle from either
		neighbor.PlayerState.DrawPile = CreatePile([village]);
		#endregion

		#region act
		player.PlayActionCardInternal(tributeToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([tribute], player.PlayerState.CardsPlayed);
		AssertPile([tribute], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([], neighbor.PlayerState.DrawPile);
		AssertPile([village], neighbor.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void TargetsTheImmediateLeftNeighborInAThreePlayerGame()
	{
		#region arrange
		var thirdPlayerUser = new Mock<IUser>();
		var thirdPlayer = CreatePlayer(game.Object, thirdPlayerUser.Object);
		game.Setup(g => g.Players).Returns([player, neighbor, thirdPlayer]);

		player.PlayerState.Hand = CreatePile([tribute]);
		var tributeToPlay = player.PlayerState.Hand[0];

		neighbor.PlayerState.DrawPile = CreatePile([village]);
		thirdPlayer.PlayerState.DrawPile = CreatePile([silver]);
		#endregion

		#region act
		player.PlayActionCardInternal(tributeToPlay);
		#endregion

		#region assert
		// only the immediate left neighbor is affected, not the player two seats away
		AssertNumbers(2, 0, 0, player);
		AssertPile([village], neighbor.PlayerState.DiscardPile);
		AssertPile([], neighbor.PlayerState.DrawPile);

		AssertPile([], thirdPlayer.PlayerState.DiscardPile);
		AssertPile([silver], thirdPlayer.PlayerState.DrawPile);
		#endregion
	}
}
