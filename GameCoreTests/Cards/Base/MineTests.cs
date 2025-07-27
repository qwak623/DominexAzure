using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MineTests
{
	private readonly Card mine = Mine.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private Kingdom kingdom;
	private PlayerState playerState;

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = new Mock<IPlayer>();

		playerState = new PlayerState(playerStateObserver: null, "Tester")
		{
			Actions = 0,
			Coins = 0,
			Buys = 0,
			Hand = new List<Card> { copper },
		};
		player.Setup(p => p.PlayerState).Returns(playerState);

		kingdom = new Kingdom(new List<Card> { mine }, 2); // todo should be mockable
		player.Setup(p => p.Game.Kingdom).Returns(kingdom);
	}

	[TestMethod]
	public void UpgradeCopperToSilver_Test()
	{
		#region setup
		player.Setup(p => p.User.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns(silver);
		// todo kingdom wrapper je asi zbytecny
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, playerState, It.IsAny<Kingdom>(),
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(copper), Times.Once);

		// user is asked to select a treasure with max price 3 to gain (it's silver)
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			playerState, It.IsAny<Kingdom>(), Phase.Gain), Times.Once);

		// silver is added to the hand
		player.Verify(p => p.GainToHand(CardType.Silver), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything_Test()
	{
		#region setup
		player.Setup(p => p.User.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns<Card>(null);
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, playerState, It.IsAny<Kingdom>(),
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player does not trash anything
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);

		// user is never asked to choose any card to gain
		player.Verify(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);

		// nothing is gained
		player.Verify(p => p.GainToHand(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything_Test()
	{
		#region setup
		player.Setup(p => p.User.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, playerState, It.IsAny<Kingdom>(),
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(copper), Times.Once);

		// user is asked to select a treasure with max price 3 to gain, but there is no such card available
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			playerState, It.IsAny<Kingdom>(), Phase.Gain), Times.Once);

		// nothing is added to the hand
		player.Verify(p => p.GainToHand(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}