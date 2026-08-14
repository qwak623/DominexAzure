using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerLurkerTests : CardWithPlayerTestsBase
{
	private readonly Card lurker = Lurker.Get();
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { lurker, village });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void TrashesActionCardFromSupply()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([lurker]);
		var lurkerToPlay = player.PlayerState.Hand[0];
		var villageInSupply = player.Game.Kingdom.GetPile(CardType.Village).CardInstance;

		user.Setup(u => u.LurkerTrash(lurker, player.PlayerState, player.Game.Kingdom)).Returns(true);
		user.Setup(u => u.LurkerChooseCardToTrash(lurker, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Contains(villageInSupply))))
			.Returns(villageInSupply);
		#endregion

		#region act
		player.PlayActionCardInternal(lurkerToPlay);
		#endregion

		#region assert
		// +1 action (lurker's own)
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([lurker], player.PlayerState.CardsPlayed);
		AssertPile([lurker], player.PlayerState.ActionsPlayed);
		AssertPile([village], player.Game.Trash);

		// the village pile in the kingdom actually lost a copy
		Assert.AreEqual(9, player.Game.Kingdom.GetPile(CardType.Village).Count);

		user.Verify(u => u.LurkerChooseCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void GainsActionCardFromTrash()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([lurker]);
		var lurkerToPlay = player.PlayerState.Hand[0];

		var villageInTrash = CreatePile([village])[0];
		player.Game.Trash.MoveRange([villageInTrash]);

		user.Setup(u => u.LurkerTrash(lurker, player.PlayerState, player.Game.Kingdom)).Returns(false);
		user.Setup(u => u.LurkerChooseCardToGain(lurker, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Single() == villageInTrash)))
			.Returns(villageInTrash);
		#endregion

		#region act
		player.PlayActionCardInternal(lurkerToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([village], player.PlayerState.DiscardPile);
		AssertPile([lurker], player.PlayerState.CardsPlayed);
		AssertPile([lurker], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.LurkerChooseCardToTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DoesNothingWhenTrashHasNoActionCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([lurker]);
		var lurkerToPlay = player.PlayerState.Hand[0];

		var copperInTrash = CreatePile([copper])[0];
		player.Game.Trash.MoveRange([copperInTrash]);

		user.Setup(u => u.LurkerTrash(lurker, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(lurkerToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([lurker], player.PlayerState.CardsPlayed);
		AssertPile([lurker], player.PlayerState.ActionsPlayed);
		AssertPile([copper], player.Game.Trash);

		user.Verify(u => u.LurkerChooseCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
