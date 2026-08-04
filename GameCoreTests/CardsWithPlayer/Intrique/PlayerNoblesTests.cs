using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerNoblesTests : CardWithPlayerTestsBase
{
	private readonly Card nobles = Nobles.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card throneRoom = ThroneRoom.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(nobles);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void ChoosesCards()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([nobles]);
		player.PlayerState.DrawPile = CreatePile([copper, copper, copper]);
		var noblesToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom)).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(noblesToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([nobles], player.PlayerState.CardsPlayed);
		AssertPile([nobles], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ChoosesActions()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([nobles]);
		var noblesToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom)).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(noblesToPlay);
		#endregion

		#region assert
		AssertNumbers(2, 0, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([nobles], player.PlayerState.CardsPlayed);
		AssertPile([nobles], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomWithDifferentOptions()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, nobles]);
		player.PlayerState.DrawPile = CreatePile([copper, copper, copper]);
		var noblesToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Nobles);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == noblesToPlay))).Returns(noblesToPlay);

		// each resolution picks a different option - the first draws cards, the second takes actions
		user.SetupSequence(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom))
			.Returns(true).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// +3 cards from the first resolution, +2 actions from the second
		AssertNumbers(2, 0, 0, player);
		AssertPile([copper, copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([nobles, throneRoom], player.PlayerState.CardsPlayed);
		AssertPile([nobles, nobles, throneRoom], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		user.Verify(u => u.NoblesChooseCards(nobles, player.PlayerState, player.Game.Kingdom), Times.Exactly(2));
		#endregion
	}
}
