using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerMasqueradeTests : CardWithPlayerTestsBase
{
	private readonly Card masquerade = Masquerade.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card moat = Moat.Get();

	private Player player;
	private Player player2;

	private Mock<IUser> user;
	private Mock<IUser> user2;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(masquerade);
		user = new Mock<IUser>();
		user2 = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
		player2 = CreatePlayer(game.Object, user2.Object);
		game.Setup(g => g.Players).Returns([player, player2]);
	}

	[TestMethod]
	public void TwoPlayersSwapCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([masquerade, copper]);
		player.PlayerState.DrawPile = CreatePile([silver, gold]);
		player2.PlayerState.Hand = CreatePile([estate]);

		var masqueradeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Masquerade);
		var copperToPass = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		var estateToPass = player2.PlayerState.Hand[0];

		user.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperToPass);
		user2.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(estateToPass);

		user.Setup(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(masqueradeToPlay);
		#endregion

		#region assert
		// +2 cards drawn, then player and player2 simultaneously swap a card
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver, gold, estate], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([masquerade], player.PlayerState.CardsPlayed);
		AssertPile([masquerade], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([copper], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);

		user.Verify(u => u.MasqueradePass(masquerade, player.PlayerState, player.Game.Kingdom,
			It.IsAny<List<CardInstance>>()), Times.Once);
		user2.Verify(u => u.MasqueradePass(masquerade, player2.PlayerState, player.Game.Kingdom,
			It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom,
			It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TrashesCardFromHandAfterTheSwap()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([masquerade, copper]);
		player2.PlayerState.Hand = CreatePile([estate]);

		var masqueradeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Masquerade);
		var copperToPass = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		var estateToPass = player2.PlayerState.Hand[0];

		user.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperToPass);
		user2.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(estateToPass);

		// targets the card just received from player2, proving the trash step sees the hand
		// after the simultaneous pass has already happened
		user.Setup(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Contains(estateToPass))))
			.Returns(estateToPass);
		#endregion

		#region act
		player.PlayActionCardInternal(masqueradeToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([masquerade], player.PlayerState.CardsPlayed);
		AssertPile([masquerade], player.PlayerState.ActionsPlayed);
		AssertPile([estate], player.Game.Trash);

		AssertPile([copper], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void NoPassingWhenOnlyOnePlayerHasCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([masquerade]);
		player.PlayerState.DrawPile = CreatePile([copper, silver]);
		// player2's hand stays empty (CardWithPlayerTestsBase default)

		var masqueradeToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(masqueradeToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([masquerade], player.PlayerState.CardsPlayed);
		AssertPile([masquerade], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);

		user.Verify(u => u.MasqueradePass(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		user2.Verify(u => u.MasqueradePass(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThreePlayersRotateCardsToTheLeft()
	{
		#region arrange
		var user3 = new Mock<IUser>();
		var player3 = CreatePlayer(game.Object, user3.Object);
		game.Setup(g => g.Players).Returns([player, player2, player3]);

		player.PlayerState.Hand = CreatePile([masquerade, copper]);
		player2.PlayerState.Hand = CreatePile([silver]);
		player3.PlayerState.Hand = CreatePile([gold]);

		var masqueradeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Masquerade);
		var copperToPass = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		var silverToPass = player2.PlayerState.Hand[0];
		var goldToPass = player3.PlayerState.Hand[0];

		user.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperToPass);
		user2.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(silverToPass);
		user3.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(goldToPass);

		user.Setup(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(masqueradeToPlay);
		#endregion

		#region assert
		// each player gives their chosen card to the next player in turn order and
		// receives whatever the previous player gave away, all at once
		AssertNumbers(0, 0, 0, player);
		AssertPile([gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([masquerade], player.PlayerState.CardsPlayed);
		AssertPile([masquerade], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([copper], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);

		AssertPile([silver], player3.PlayerState.Hand);
		AssertPile([], player3.PlayerState.DrawPile);
		AssertPile([], player3.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void MoatDoesNotBlockMasquerade()
	{
		#region arrange
		// masquerade is not an attack card, so moat should never get a chance to block it
		player.PlayerState.Hand = CreatePile([masquerade, copper]);
		player.PlayerState.DrawPile = CreatePile([silver, gold]);
		player2.PlayerState.Hand = CreatePile([moat]);

		var masqueradeToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Masquerade);
		var copperToPass = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper);
		var moatToPass = player2.PlayerState.Hand[0];

		user.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(copperToPass);
		user2.Setup(u => u.MasqueradePass(masquerade, It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()))
			.Returns(moatToPass);

		user.Setup(u => u.MasqueradeTrash(masquerade, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(masqueradeToPlay);
		#endregion

		#region assert
		// player2 still has to pass a card like anyone else - moat only reveals against attacks
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver, gold, moat], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([masquerade], player.PlayerState.CardsPlayed);
		AssertPile([masquerade], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		AssertPile([copper], player2.PlayerState.Hand);
		AssertPile([], player2.PlayerState.DrawPile);
		AssertPile([], player2.PlayerState.DiscardPile);

		user2.Verify(u => u.MasqueradePass(masquerade, player2.PlayerState, player.Game.Kingdom,
			It.IsAny<List<CardInstance>>()), Times.Once);

		// player2 is never even asked about a reaction - masquerade never calls DealAttack
		user2.Verify(u => u.PlayCard(It.IsAny<List<CardInstance>>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), Phase.Reaction, It.IsAny<Card>()), Times.Never);
		#endregion
	}

	[TestMethod]
	[Ignore("TODO: once gain hooks exist (see Player.Gain's 'TODO hook on gain event'), add a test " +
		"proving cards passed via masquerade do NOT trigger them - passing a card to another " +
		"player is not the same as gaining one, and this is easy to get wrong/forget.")]
	public void PassedCardsDoNotTriggerGainHooks()
	{
		Assert.Inconclusive("Not implemented - no gain hooks exist yet to verify against.");
	}
}
