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
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);

		// selection is pulled from the wrapper itself, so this only succeeds if laboratory
		// ($5) genuinely passes the wrapper's own availability check at the computed threshold
		KingdomWrapper wrapper = null;
		attackerUser.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Laboratory));
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

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Laboratory));
		attackerUser.Verify(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		attackerUser.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GainedTreasureCardGoesOntoTheDrawPile()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, laboratory]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Replace);
		var laboratoryInHand = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Laboratory);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(laboratoryInHand);

		KingdomWrapper wrapper = null;
		attackerUser.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Gold));
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

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Gold));
		attackerUser.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 7 && !kw.OnlyTreasures),
			attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void GainedVictoryCardStaysInDiscardAndAttacksOtherPlayers()
	{
		#region arrange
		attacker.PlayerState.Hand = CreatePile([replace, silver]);
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);
		attackerUser.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain))
			.Returns<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => kw.GetCard(CardType.Duchy));
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
		var replaceToPlay = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Replace);
		var silverInHand = attacker.PlayerState.Hand.First(c => c.Card.Type == CardType.Silver);

		attackerUser.Setup(u => u.ReplaceTrash(replace, attacker.PlayerState, attacker.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);
		attackerUser.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), attacker.PlayerState, attacker.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
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
		attackerUser.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);
		#endregion
	}
}
