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

		var duchyToGain = player.Game.Kingdom.GetPile(CardName.Duchy).CardInstance;
		user.Setup(u => u.SelectCardToGain(feast, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
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
		user.Verify(u => u.SelectCardToGain(feast, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) <= 5))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([feast]);
		EmptyKingdom();
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

		// nothing is available, so the user is never asked to choose a card to gain
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainDuchyAndFeast()
	{
		// TODO hostina je dvakrát na smetišti

		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, feast]);

		var duchyToGain = player.Game.Kingdom.GetPile(CardName.Duchy).CardInstance;
		var feastToGain = player.Game.Kingdom.GetPile(CardName.Feast).CardInstance;
		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Feast);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		user.SetupSequence(u => u.SelectCardToGain(feast, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(duchyToGain).Returns(feastToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([duchy, feast], player.PlayerState.DiscardPile);
		AssertPile([throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, feast, feast], player.PlayerState.ActionsPlayed);
		AssertPile([feast], player.Game.Trash);

		// user has to select a card with price max 5 to gain
		user.Verify(u => u.SelectCardToGain(feast, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) <= 5))), Times.Exactly(2));

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

		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Feast);

		// only a single duchy left: the first feast resolution takes it (sole candidate, no
		// prompt), the second resolution finds nothing to gain
		EmptyKingdom(except: CardName.Duchy);
		SetKingdomPileCount(CardName.Duchy, 1);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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

		// duchy is the sole candidate on the first resolution and there are none on the
		// second, so the user is never actually prompted
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, feast]);
		var feastToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Feast);
		EmptyKingdom();
		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == feastToPlay))).Returns(feastToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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

		// nothing is available on either resolution, so the user is never prompted
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}