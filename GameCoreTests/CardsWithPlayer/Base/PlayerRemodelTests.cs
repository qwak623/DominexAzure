using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerRemodelTests : CardWithPlayerTestsBase
{
	private readonly Card remodel = Remodel.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card laboratory = Laboratory.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([remodel, laboratory]);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void UpgradeSilverToLaboratory()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, laboratory]);
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Remodel);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand);

		// selection is pulled from the candidate list itself, so this only succeeds if
		// laboratory ($5) genuinely passes the computed price threshold
		List<CardInstance> availableCards = null;
		user.Setup(u => u.SelectCardToGain(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Callback<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) => availableCards = cards)
			.Returns(() => availableCards.SingleOrDefault(c => c.Card.Name == CardName.Laboratory));
		#endregion

		#region act
		player.PlayActionCardInternal(remodelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([laboratory], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([laboratory], player.PlayerState.DiscardPile);
		AssertPile([remodel], player.PlayerState.CardsPlayed);
		AssertPile([remodel], player.PlayerState.ActionsPlayed);
		AssertPile([silver], player.Game.Trash);

		Assert.IsTrue(availableCards.Any(c => c.Card.Name == CardName.Laboratory));
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.SelectCardToGain(remodel, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.All(x => x.Card.GetPrice(player.PlayerState) <= 5))), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel]);
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Remodel);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>())).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(remodelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([remodel], player.PlayerState.CardsPlayed);
		AssertPile([remodel], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Never);

		// nothing was trashed, so the user is never asked to choose anything to gain
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, laboratory]);
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Remodel);
		var laboratoryInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Laboratory);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(laboratoryInHand);
		// nothing in the supply to gain
		EmptyKingdom();
		#endregion

		#region act
		player.PlayActionCardInternal(remodelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([remodel], player.PlayerState.CardsPlayed);
		AssertPile([remodel], player.PlayerState.ActionsPlayed);
		AssertPile([laboratory], player.Game.Trash);

		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);

		// the supply is empty, so the user is never asked which card to gain
		user.Verify(u => u.SelectCardToGain(It.IsAny<Card>(), It.IsAny<PlayerState>(),
			It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, throneRoom, throneRoom]);
		var throneRoomsInHand = player.PlayerState.Hand.Where(c => c.Card.Name == CardName.ThroneRoom).ToList();
		var throneRoomToPlay = throneRoomsInHand[0];
		var throneRoomFodder = throneRoomsInHand[1];
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Remodel);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Silver);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<List<CardInstance>>(c => c.Contains(remodelToPlay)))).Returns(remodelToPlay);
		user.SetupSequence(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns(silverInHand).Returns(throneRoomFodder);

		// each resolution's selection is pulled from its own candidate list, so this only
		// succeeds if laboratory ($5) and gold ($6) genuinely pass their respective computed
		// price thresholds
		var expectedGains = new Queue<CardName>([CardName.Laboratory, CardName.Gold]);
		var seenCandidates = new List<List<CardInstance>>();
		user.Setup(u => u.SelectCardToGain(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()))
			.Returns<Card, PlayerState, Kingdom, List<CardInstance>>((c, ps, k, cards) =>
			{
				seenCandidates.Add(cards);
				var name = expectedGains.Dequeue();
				return cards.SingleOrDefault(x => x.Card.Name == name);
			});
		#endregion

		#region act
		player.PlayActionCardInternal(throneRoomToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([laboratory, gold], player.PlayerState.DiscardPile);
		AssertPile([remodel, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([remodel, remodel, throneRoom], player.PlayerState.ActionsPlayed);

		// the second resolution trashes the throne room still sitting in hand - the one that
		// wasn't itself used to play remodel twice
		AssertPile([silver, throneRoom], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		user.Verify(u => u.SelectCardToGain(remodel, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Max(x => x.Card.GetPrice(player.PlayerState)) == 5)), Times.Once);
		user.Verify(u => u.SelectCardToGain(remodel, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Max(x => x.Card.GetPrice(player.PlayerState)) == 6)), Times.Once);

		Assert.IsTrue(seenCandidates[0].Any(c => c.Card.Name == CardName.Laboratory));
		Assert.IsTrue(seenCandidates[1].Any(c => c.Card.Name == CardName.Gold));
		#endregion
	}
}
