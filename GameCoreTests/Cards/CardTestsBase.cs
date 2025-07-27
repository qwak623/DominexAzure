using GameCore;
using GameCore.Cards;
using Moq;

namespace GameCoreTests.Cards;
public class CardTestsBase
{
	public Kingdom MockKingdom(Card card)
	{
		return new Kingdom(new List<Card> { card }, 2); // todo should be mockable
	}

	public Mock<IPlayer> MockPlayer(Kingdom kingdom)
	{
		var player = new Mock<IPlayer>();

		var playerState = new PlayerState(playerStateObserver: null, "Tester")
		{
			Actions = 0,
			Coins = 0,
			Buys = 0,
			Hand = new List<Card> { },
		};
		player.Setup(p => p.PlayerState).Returns(playerState);
		player.Setup(p => p.Game.Kingdom).Returns(kingdom);

		return player;
	}
}
