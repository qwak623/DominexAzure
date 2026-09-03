using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerReplaceTests : CardWithPlayerTestsBase
{
	private readonly Card replace = Replace.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card curse = Curse.Get();
	private readonly Card laboratory = Laboratory.Get();

	private Player attacker;
	private Player defender;

	private Mock<IUser> attackerUser;
	private Mock<IUser> defenderUser;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([replace, laboratory]);

		attackerUser = new Mock<IUser>();
		defenderUser = new Mock<IUser>();
		attacker = CreatePlayer(game.Object, attackerUser.Object);
		defender = CreatePlayer(game.Object, defenderUser.Object);

		game.Setup(g => g.Players).Returns(new List<IPlayer> { attacker, defender });
	}

	[TestMethod]
	public void GainedActionCardGoesOntoTheDrawPile()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, silver]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);

		// selection is pulled from the candidate list itself, so this only succeeds if
		// laboratory ($5) genuinely passes the computed price threshold
		List<CardInstance> availableCards = null;
		attackerUser.Setup(u => u.SelectCardToGain(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.SingleOrDefault(c => c.Card.Name == CardName.Laboratory));
		#endregion

		#region act
		attacker.PlayActionCardInternal(replaceToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([laboratory], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([replace], attacker.PlayerState.CardsPlayed);
		AssertPile([replace], attacker.PlayerState.ActionsPlayed);
		AssertPile([silver], attacker.Game.Trash);

		// gaining an Action card does not attack the other player
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);

		Assert.Contains(c => c.Card.Name == CardName.Laboratory, availableCards);
		attackerUser.Verify(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Never);
		attackerUser.Verify(u => u.SelectCardToGain(replace, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(attacker.PlayerState) <= 5))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GainedTreasureCardGoesOntoTheDrawPile()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, laboratory]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Replace);
		var laboratoryInHand = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Laboratory);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(laboratoryInHand);

		List<CardInstance> availableCards = null;
		attackerUser.Setup(u => u.SelectCardToGain(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.SingleOrDefault(c => c.Card.Name == CardName.Gold));
		#endregion

		#region act
		attacker.PlayActionCardInternal(replaceToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([gold], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([replace], attacker.PlayerState.CardsPlayed);
		AssertPile([replace], attacker.PlayerState.ActionsPlayed);
		AssertPile([laboratory], attacker.Game.Trash);

		// gaining a Treasure card does not attack the other player
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);

		Assert.IsTrue(availableCards.Any(c => c.Card.Name == CardName.Gold));
		attackerUser.Verify(u => u.SelectCardToGain(replace, attacker.PlayerState, attacker.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(attacker.PlayerState) <= 7))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GainedVictoryCardStaysInDiscardAndAttacksOtherPlayers()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, silver]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);
		attackerUser.Setup(u => u.SelectCardToGain(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => cards.SingleOrDefault(x => x.Card.Name == CardName.Duchy));
		#endregion

		#region act
		attacker.PlayActionCardInternal(replaceToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		// unlike Action/Treasure gains, a gained Victory card is not moved onto the draw pile -
		// it stays wherever Gain put it
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([duchy], attacker.PlayerState.DiscardPile);
		AssertPile([replace], attacker.PlayerState.CardsPlayed);
		AssertPile([replace], attacker.PlayerState.ActionsPlayed);
		AssertPile([silver], attacker.Game.Trash);

		// gaining a Victory card attacks every other player with a Curse
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([curse], defender.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, silver]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);
		// nothing in the supply to gain
		EmptyKingdom();
		#endregion

		#region act
		attacker.PlayActionCardInternal(replaceToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([replace], attacker.PlayerState.CardsPlayed);
		AssertPile([replace], attacker.PlayerState.ActionsPlayed);
		AssertPile([silver], attacker.Game.Trash);

		// no card was gained, so no attack happens either
		AssertPile([], defender.PlayerState.Hand);
		AssertPile([], defender.PlayerState.DrawPile);
		AssertPile([], defender.PlayerState.DiscardPile);

		attackerUser.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void NoCardsToTrash()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace]);
		var replaceToPlay = attacker.PlayerState.Hand[0];
		#endregion

		#region act
		attacker.PlayActionCardInternal(replaceToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, attacker);
		AssertPile([], attacker.PlayerState.Hand);
		AssertPile([], attacker.PlayerState.DrawPile);
		AssertPile([], attacker.PlayerState.DiscardPile);
		AssertPile([replace], attacker.PlayerState.CardsPlayed);
		AssertPile([replace], attacker.PlayerState.ActionsPlayed);
		AssertPile([], attacker.Game.Trash);

		attackerUser.Verify(u => u.ReplaceTrash(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		attackerUser.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
