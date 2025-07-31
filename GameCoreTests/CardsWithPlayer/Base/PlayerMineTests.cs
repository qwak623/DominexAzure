using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMineTests : CardWithPlayerTestsBase
{
	private readonly Card mine = Mine.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(mine);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void UpgradeCopperToSilver()
	{
		#region arrange
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns(silver);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// copper was trashed
		CollectionAssert.AreEqual(new List<Card> { copper }, player.Game.Trash.ToList());

		// silver was gained to hand
		CollectionAssert.AreEqual(new List<Card> { silver }, player.PlayerState.Hand);

		// mine was added to played cards
		CollectionAssert.AreEqual(new List<Card> { mine }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns<Card>(null);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// nothing was trashed
		Assert.IsFalse(player.Game.Trash.Any());

		// copper stayed in hand
		CollectionAssert.AreEqual(new List<Card> { copper }, player.PlayerState.Hand);

		// mine was added to played cards
		CollectionAssert.AreEqual(new List<Card> { mine }, player.PlayerState.PlayedCards);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns<Card>(null);

		player.PlayerState.Hand = new List<Card> { mine, copper };
		#endregion

		#region act
		player.PlayActionCardInternal(mine);
		#endregion

		#region assert
		// -1 Action
		Assert.AreEqual(0, player.PlayerState.Actions);

		// coins and buys shouldn't change
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);

		// copper was trashed
		CollectionAssert.AreEqual(new List<Card> { copper }, player.Game.Trash.ToList());

		// nothing was gained to hand
		Assert.IsFalse(player.PlayerState.Hand.Any());

		// mine was added to played cards
		CollectionAssert.AreEqual(new List<Card> { mine }, player.PlayerState.PlayedCards);
		#endregion
	}
}