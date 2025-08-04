using GameCore.Cards;
using Moq;

namespace GameCore.CardWithPlayer.Tests;
public class CardWithPlayerTestsBase
{
	public Mock<IGame> MockGame(Card card)
	{
		return MockGame(new List<Card> { card });
	}

	public Mock<IGame> MockGame(List<Card> cards)
	{
		var kingdom = new Kingdom(cards, 2); // todo should be mockable

		var game = new Mock<IGame>();
		game.Setup(g => g.Kingdom).Returns(kingdom);
		game.Setup(g => g.Trash).Returns(new List<Card> { });
		return game;
	}

	public Player CreatePlayer(IGame game, IUser user)
	{
		var player = new Player(game, user);
		player.PlayerState.Actions = 1;
		player.PlayerState.Buys = 0;
		player.PlayerState.Coins = 0;
		player.PlayerState.Hand = new List<Card> { };
		player.PlayerState.PlayedCards = new List<Card> { };
		player.PlayerState.DrawPile = new List<Card> { };
		player.PlayerState.DiscardPile = new List<Card> { };
		return player;
	}
}
