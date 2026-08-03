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
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Remodel);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Silver);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(silverInHand);

		// selection is pulled from the wrapper itself, so this only succeeds if laboratory
		// ($5) genuinely passes the wrapper's own availability check at the computed threshold
		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardType.Laboratory));
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

		Assert.IsTrue(wrapper.AvailableCards.Any(c => c.Card.Type == CardType.Laboratory));
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, laboratory]);
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Remodel);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom)).Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(remodelToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([silver, laboratory], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([remodel], player.PlayerState.CardsPlayed);
		AssertPile([remodel], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// nothing was trashed, so the user is never asked to choose anything to gain
		user.Verify(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, laboratory]);
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Remodel);
		var laboratoryInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Laboratory);

		user.Setup(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(laboratoryInHand);
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
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

		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Once);

		// max price 7 (laboratory costs 5, +2), but nothing is selected
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 7 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([remodel, silver, throneRoom, throneRoom]);
		var throneRoomsInHand = player.PlayerState.Hand.Where(c => c.Card.Type == CardType.ThroneRoom).ToList();
		var throneRoomToPlay = throneRoomsInHand[0];
		var throneRoomFodder = throneRoomsInHand[1];
		var remodelToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Remodel);
		var silverInHand = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Silver);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Contains(remodelToPlay)))).Returns(remodelToPlay);
		user.SetupSequence(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom))
			.Returns(silverInHand).Returns(throneRoomFodder);

		// each resolution's selection is pulled from its own wrapper, so this only succeeds
		// if laboratory ($5) and gold ($6) genuinely pass the wrapper's own availability
		// check at their respective computed thresholds
		var expectedGains = new Queue<CardType>([CardType.Laboratory, CardType.Gold]);
		var wrappers = new List<KingdomWrapper>();
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) =>
			{
				wrappers.Add(kw);
				return kw.GetCard(expectedGains.Dequeue());
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
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		user.Verify(u => u.RemodelTrash(remodel, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);
		user.Verify(u => u.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 6 && !kw.OnlyTreasures),
			player.PlayerState, player.Game.Kingdom, Phase.Gain), Times.Once);

		Assert.IsTrue(wrappers[0].AvailableCards.Any(c => c.Card.Type == CardType.Laboratory));
		Assert.IsTrue(wrappers[1].AvailableCards.Any(c => c.Card.Type == CardType.Gold));
		#endregion
	}
}
