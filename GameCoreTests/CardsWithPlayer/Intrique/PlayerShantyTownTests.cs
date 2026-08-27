using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerShantyTownTests : CardWithPlayerTestsBase
{
	private readonly Card shantyTown = ShantyTown.Get();
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(shantyTown);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void DrawsTwoWhenNoActionCardsInHand()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([shantyTown, copper, estate]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var shantyTownToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.ShantyTown);
		#endregion

		#region act
		player.PlayActionCardInternal(shantyTownToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([copper, estate, silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([shantyTown], player.PlayerState.CardsPlayed);
		AssertPile([shantyTown], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void DrawsTwoWhenHandIsEmpty()
	{
		#region arrange
		// an empty hand vacuously has "no action cards", so this should still draw
		player.PlayerState.Hand = CreatePile([shantyTown]);
		player.PlayerState.DrawPile = CreatePile([silver, silver]);
		var shantyTownToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(shantyTownToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([silver, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([shantyTown], player.PlayerState.CardsPlayed);
		AssertPile([shantyTown], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void DoesNotDrawWhenHandHasActionCard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([shantyTown, village, copper]);
		var shantyTownToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.ShantyTown);
		#endregion

		#region act
		player.PlayActionCardInternal(shantyTownToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([village, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([shantyTown], player.PlayerState.CardsPlayed);
		AssertPile([shantyTown], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomSecondResolutionSeesActionCardDrawnByTheFirst()
	{
		#region arrange
		// hand starts with no action cards, so the first resolution draws - and one of the
		// two cards it draws is an action card, so the second resolution should see it and
		// correctly not draw again
		player.PlayerState.Hand = CreatePile([throneRoom, shantyTown, copper]);
		player.PlayerState.DrawPile = CreatePile([village, silver]);
		var shantyTownToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.ShantyTown);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == shantyTownToPlay))).Returns(shantyTownToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		// +2 actions twice (once per resolution); only the first resolution draws
		AssertNumbers(4, 0, 0, player);
		AssertPile([copper, village, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([shantyTown, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([shantyTown, shantyTown, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}
}
