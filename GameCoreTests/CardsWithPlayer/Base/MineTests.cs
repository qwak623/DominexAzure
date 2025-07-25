using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class MineTests
{
	[TestMethod]
	public void UpgradeCopperToSilver_Test()
	{
		// setup
		Card mine = Mine.Get();
		Card copper = Copper.Get();
		Card silver = Silver.Get();

		var kingdom = new Kingdom(new() { mine }, 2);

		var game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);

		game.Setup(g => g.Trash.Add(It.IsAny<Card>()));

		var user = new Mock<IUser>();
		user.Setup(u => u.MineTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<IList<Card>>()))
			.Returns(copper);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), Phase.Gain))
			.Returns(silver);

		var player = new Player(game.Object, user.Object);
		player.PlayerState.Actions = 1;
		player.PlayerState.Buys = 0;
		player.PlayerState.Coins = 0;
		player.PlayerState.Hand = new List<Card> { mine, copper };

		// act
		player.PlayActionCardInternal(mine);

		// assert
		Assert.AreEqual(0, player.PlayerState.Actions);
		Assert.AreEqual(0, player.PlayerState.Coins);
		Assert.AreEqual(0, player.PlayerState.Buys);
		game.Verify(g => g.Trash.Add(It.IsAny<Copper>()), Times.Once);

		CollectionAssert.AreEqual(player.PlayerState.Hand, new List<Card> { silver });
	}
}