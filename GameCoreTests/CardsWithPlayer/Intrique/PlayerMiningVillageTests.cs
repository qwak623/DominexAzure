using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerMiningVillageTests : CardWithPlayerTestsBase
{
	private readonly Card miningVillage = MiningVillage.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(miningVillage);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void TrashesForCoins()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([miningVillage]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var miningVillageToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay))
			.Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(miningVillageToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 2, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([miningVillage], player.PlayerState.ActionsPlayed);
		AssertPile([miningVillage], player.Game.Trash);

		user.Verify(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DoesNotTrash()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([miningVillage]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var miningVillageToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay))
			.Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(miningVillageToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([miningVillage], player.PlayerState.CardsPlayed);
		AssertPile([miningVillage], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomTrashesOnlyOnce()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, miningVillage]);
		player.PlayerState.DrawPile = CreatePile([copper, silver]);
		var miningVillageToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.MiningVillage);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == miningVillageToPlay))).Returns(miningVillageToPlay);

		// even if the user were asked (and said yes) on both resolutions, the guard in
		// MiningVillage.ActionEffect should stop the second resolution from asking again -
		// or trashing/paying out again - once the card is already sitting in the trash pile
		user.Setup(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay))
			.Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// +2 actions and +1 card fire on both resolutions, but the +2 coins and the trash
		// itself only happen once
		AssertNumbers(4, 2, 0, player);
		AssertPile([copper, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, miningVillage, miningVillage], player.PlayerState.ActionsPlayed);
		AssertPile([miningVillage], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.MiningVillageTrash(miningVillage, player.PlayerState, player.Game.Kingdom, miningVillageToPlay), Times.Once);
		#endregion
	}
}
