using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerFeastTests : CardWithPlayerTestsBase
{
	private readonly Card feast = Feast.Get();
	private readonly Card duchy = Duchy.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(feast);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainDuchy()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([feast]);

		var duchyToGain = new CardInstance(duchy, new Pile(), 10);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(duchyToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		// the feast was transferred from played cards to trash 
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([duchy], player.PlayerState.DiscardPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([feast]);

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain)).Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand[0]);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([], player.PlayerState.CardsPlayed);
		AssertPile([feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainTwoDuchies()
	{
		// TODO hostina je dvakrát na smetišti

		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, feast]);

		var duchyToGain = new CardInstance(duchy, new Pile(), 10);
		var duchyToGain2 = new CardInstance(duchy, new Pile(), 11);
		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Feast);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(duchyToGain).Returns(duchyToGain2);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([duchy, duchy], player.PlayerState.DiscardPile);
		AssertPile([throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, feast, feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneDuchyAvailable()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, feast]);

		var duchyToGain = new CardInstance(duchy, new Pile(), 10);
		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Feast);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(duchyToGain).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([duchy], player.PlayerState.DiscardPile);
		AssertPile([throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, feast, feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// user has to select a card with price max 5 to gain - there is only one duchy
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, feast]);
		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Feast);
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, feast, feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user was asked which card to play using throne room
		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// user has to select a card with price max 5 to gain - there is none
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.Price == 5 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}
}