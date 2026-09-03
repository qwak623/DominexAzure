using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerUpgradeTests : CardWithPlayerTestsBase
{
	private readonly Card upgrade = Upgrade.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card smithy = Smithy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([upgrade, smithy, throneRoom]);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void TrashesCardAndGainsOneCostHigher()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([upgrade]);
		player.PlayerState.DrawPile = CreatePile([silver]);
		var upgradeToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.UpgradeTrash(upgrade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards.Single());

		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(upgrade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.SingleOrDefault(c => c.Card.Name == CardName.Smithy));
		#endregion

		#region act
		player.PlayActionCardInternal(upgradeToPlay);
		#endregion

		#region assert
		// +1 Card (drawn silver, then trashed) and +1 Action cancel out playing upgrade itself
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([smithy], player.PlayerState.DiscardPile);
		AssertPile([upgrade], player.PlayerState.CardsPlayed);
		AssertPile([upgrade], player.PlayerState.ActionsPlayed);
		AssertPile([silver], player.Game.Trash);

		// gaining is restricted to exactly $1 more than the trashed card, not "up to"
		Assert.IsTrue(availableCards.All(c => c.Card.GetPrice(player.PlayerState) == 4));
		user.Verify(u => u.SelectCardToGain(upgrade, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) == 4))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([upgrade]);
		player.PlayerState.DrawPile = CreatePile([]);
		var upgradeToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(upgradeToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.UpgradeTrash(It.IsAny<Card>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnythingWhenNoCardOneCostHigherIsAvailable()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([upgrade]);
		// gold costs $6, and nothing in this kingdom (upgrade $5, smithy $4, plus the basic
		// treasures/victories always added) costs $7, so there is genuinely nothing to gain -
		// the player isn't just choosing to decline a real option
		player.PlayerState.DrawPile = CreatePile([gold]);
		var upgradeToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.UpgradeTrash(upgrade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards.Single());
		// nothing in the kingdom costs exactly $7, so the candidate list is genuinely empty
		user.Setup(u => u.SelectCardToGain(upgrade, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 0))).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(upgradeToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([gold], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void NoCardsToTrashWhenHandIsEmptyAfterDrawing()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([upgrade]);
		player.PlayerState.DrawPile = CreatePile([]);
		player.PlayerState.DiscardPile = CreatePile([]);
		var upgradeToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(upgradeToPlay);
		#endregion

		#region assert
		// +1 Action is still applied even though there was nothing left to draw
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.UpgradeTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
