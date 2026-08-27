using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerSecretPassageTests : CardWithPlayerTestsBase
{
	private readonly Card secretPassage = SecretPassage.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card estate = Estate.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(secretPassage);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void MovesTheChosenCardBackOntoTheDrawPile()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([secretPassage, estate]);
		// bottom to top: both get drawn off by the +2 Cards effect before the chosen-card
		// selection runs
		player.PlayerState.DrawPile = CreatePile([silver, copper]);
		var secretPassageToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.SecretPassage);

		user.Setup(u => u.SecretPassageChooseCard(secretPassage, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards.Single(x => x.Card.Name == CardName.Estate));
		#endregion

		#region act
		player.PlayActionCardInternal(secretPassageToPlay);
		#endregion

		#region assert
		// +1 Action cancels out playing secret passage itself
		AssertNumbers(1, 0, 0, player);
		AssertPile([copper, silver], player.PlayerState.Hand);
		AssertPile([estate], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([secretPassage], player.PlayerState.CardsPlayed);
		AssertPile([secretPassage], player.PlayerState.ActionsPlayed);

		// the whole post-draw hand is offered, not just the newly drawn cards
		user.Verify(u => u.SecretPassageChooseCard(secretPassage, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 3)), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingHappensWhenNoCardsAreLeftInHandAfterDrawing()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([secretPassage]);
		player.PlayerState.DrawPile = CreatePile([]);
		player.PlayerState.DiscardPile = CreatePile([]);
		var secretPassageToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(secretPassageToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);

		user.Verify(u => u.SecretPassageChooseCard(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
