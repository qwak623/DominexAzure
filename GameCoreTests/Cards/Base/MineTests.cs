using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MineTests
{
	[TestMethod]
	public void UpgradeCopperToSilver_Test()
	{
		#region setup
		Card mine = Mine.Get();
		Card copper = Copper.Get();
		Card silver = Silver.Get();

		var user = new Mock<IUser>();
		var game = new Mock<IGame>();

		var kingdom = new Kingdom(new List<Card> { mine }, 2); // todo should be mockable

		var playerState = new PlayerState(playerStateObserver: null, "Tester")
		{
			Actions = 0,
			Coins = 0,
			Buys = 0,
			Hand = new List<Card> { copper },
		};

		var player = new Mock<IPlayer>();

		player.Setup(p => p.PlayerState).Returns(playerState);
		player.Setup(p => p.User.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns(silver);
		player.Setup(p => p.Game.Kingdom).Returns(kingdom);

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

		// user is asked to chose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, playerState, It.IsAny<Kingdom>(),
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(copper), Times.Once);

		// user is asked to select a treasure with max price 3 to gain (its silver)
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			playerState, It.IsAny<Kingdom>(), Phase.Gain), Times.Once);

		// silver is added to the hand
		player.Verify(p => p.GainToHand(CardType.Silver));
		#endregion
	}
}