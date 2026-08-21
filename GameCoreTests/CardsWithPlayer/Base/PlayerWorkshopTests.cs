using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerWorkshopTests : CardWithPlayerTestsBase
{
	private readonly Card workshop = Workshop.Get();
	private readonly Card village = Village.Get();
	private readonly Card moat = Moat.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(new List<Card> { workshop, village, moat });
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainVillage()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([workshop]);
		var workshopToPlay = player.PlayerState.Hand[0];
		var villageToGain = player.Game.Kingdom.GetPile(CardType.Village).CardInstance;

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(villageToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(workshopToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([village], player.PlayerState.DiscardPile);
		AssertPile([workshop], player.PlayerState.CardsPlayed);
		AssertPile([workshop], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.MaxPrice == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([workshop]);
		var workshopToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(workshopToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([workshop], player.PlayerState.CardsPlayed);
		AssertPile([workshop], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.MaxPrice == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void AvailableCardsAreLimitedToPriceFour()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([workshop]);
		var workshopToPlay = player.PlayerState.Hand[0];

		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => availableCards = kw.AvailableCards.ToList())
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(workshopToPlay);
		#endregion

		#region assert
		// silver ($3) is within workshop's cap, duchy ($5) is not
		Assert.IsTrue(availableCards.Any(c => c.Card.Type == CardType.Silver));
		Assert.IsFalse(availableCards.Any(c => c.Card.Type == CardType.Duchy));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainVillageAndMoat()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Workshop);
		var villageToGain = player.Game.Kingdom.GetPile(CardType.Village).CardInstance;
		var moatToGain = player.Game.Kingdom.GetPile(CardType.Moat).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(villageToGain).Returns(moatToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([village, moat], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, workshop], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, workshop, workshop], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.MaxPrice == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneVillageAvailable()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Workshop);
		var villageToGain = player.Game.Kingdom.GetPile(CardType.Village).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		user.SetupSequence(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns(villageToGain).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([village], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, workshop], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, workshop, workshop], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.MaxPrice == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Workshop);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, workshop], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, workshop, workshop], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(k => k.MaxPrice == 4 && k.OnlyTreasures == false),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Exactly(2));
		#endregion
	}
}
