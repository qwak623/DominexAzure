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
		var villageToGain = player.Game.Kingdom.GetPile(CardName.Village).CardInstance;

		user.Setup(u => u.SelectCardToGain(workshop, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
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

		user.Verify(u => u.SelectCardToGain(workshop, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) <= 4))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void NothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([workshop]);
		var workshopToPlay = player.PlayerState.Hand[0];

		// every pile is empty, so there is genuinely nothing to gain
		EmptyKingdom();
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

		// with no candidates the user is never asked to choose
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void AvailableCardsAreLimitedToPriceFour()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([workshop]);
		var workshopToPlay = player.PlayerState.Hand[0];

		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(workshop, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.First(c => c.Card.Name == CardName.Silver));
		#endregion

		#region act
		player.PlayActionCardInternal(workshopToPlay);
		#endregion

		#region assert
		// silver ($3) is within workshop's cap, duchy ($5) is not
		Assert.IsTrue(availableCards.Any(c => c.Card.Name == CardName.Silver));
		Assert.IsFalse(availableCards.Any(c => c.Card.Name == CardName.Duchy));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomGainVillageAndMoat()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Workshop);
		var villageToGain = player.Game.Kingdom.GetPile(CardName.Village).CardInstance;
		var moatToGain = player.Game.Kingdom.GetPile(CardName.Moat).CardInstance;

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		user.SetupSequence(u => u.SelectCardToGain(workshop, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(villageToGain).Returns(moatToGain);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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
		user.Verify(u => u.SelectCardToGain(workshop, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) <= 4))), Times.Exactly(2));
		#endregion
	}

	[TestMethod]
	public void ThroneRoomOneVillageAvailable()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Workshop);

		// only a single village left in the supply: the first workshop resolution takes it
		// (sole candidate, so the user isn't prompted), the second finds nothing
		EmptyKingdom(except: CardName.Village);
		SetKingdomPileCount(CardName.Village, 1);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomNothingToGain()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, workshop]);
		var workshopToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Workshop);

		EmptyKingdom();

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Single() == workshopToPlay))).Returns(workshopToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Name == CardName.ThroneRoom));
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
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
