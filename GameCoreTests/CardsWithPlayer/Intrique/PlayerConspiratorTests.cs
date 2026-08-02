using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerConspiratorTests : CardWithPlayerTestsBase
{
	private readonly Card conspirator = Conspirator.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(conspirator);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PlayAsFirstCard()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([conspirator]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var conspiratorToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(conspiratorToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([conspirator], player.PlayerState.CardsPlayed);
		AssertPile([conspirator], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void PlayAsSecondCard()
	{
		#region arrange
		// one conspirator already played this turn, played again below - still under the
		// threshold of 3, so no bonus yet
		player.PlayerState.CardsPlayed = CreatePile([conspirator]);
		player.PlayerState.ActionsPlayed = [conspirator];
		player.PlayerState.Hand = CreatePile([conspirator]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var conspiratorToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(conspiratorToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([conspirator, conspirator], player.PlayerState.CardsPlayed);
		AssertPile([conspirator, conspirator], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void PlayAsThirdCard()
	{
		#region arrange
		// two conspirators already played this turn; playing this one brings the count to 3
		// (counting itself), which triggers the +1 Card, +1 Action bonus
		player.PlayerState.CardsPlayed = CreatePile([conspirator, conspirator]);
		player.PlayerState.ActionsPlayed = [conspirator, conspirator];
		player.PlayerState.Hand = CreatePile([conspirator]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var conspiratorToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(conspiratorToPlay);
		#endregion

		#region assert
		AssertNumbers(1, 2, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([conspirator, conspirator, conspirator], player.PlayerState.CardsPlayed);
		AssertPile([conspirator, conspirator, conspirator], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsFirst()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([throneRoom, conspirator]);
		player.PlayerState.DrawPile = CreatePile([copper, copper]);
		var conspiratorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Conspirator);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == conspiratorToPlay))).Returns(conspiratorToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		// throne room itself counts as the first played action, so the count only reaches 3
		// (and the bonus fires) on the second of the two conspirator resolutions
		AssertNumbers(1, 4, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([copper], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([throneRoom, conspirator], player.PlayerState.CardsPlayed);
		AssertPile([throneRoom, conspirator, conspirator], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlayAsSecond()
	{
		#region arrange
		// one conspirator already played this turn - throne room's own count plus both
		// conspirator resolutions push the count to 3 and then 4, so the bonus fires both times
		player.PlayerState.CardsPlayed = CreatePile([conspirator]);
		player.PlayerState.ActionsPlayed = [conspirator];
		player.PlayerState.Hand = CreatePile([throneRoom, conspirator]);
		player.PlayerState.DrawPile = CreatePile([copper, copper]);
		var conspiratorToPlay = player.PlayerState.Hand.First(c => c.Card.Type == CardType.Conspirator);

		user.Setup(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.Is<IEnumerable<CardInstance>>(c => c.Single() == conspiratorToPlay))).Returns(conspiratorToPlay);
		#endregion

		#region act
		player.PlayActionCardInternal(player.PlayerState.Hand.First(c => c.Card.Type == CardType.ThroneRoom));
		#endregion

		#region assert
		AssertNumbers(2, 4, 0, player);
		AssertPile([copper, copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([conspirator, throneRoom, conspirator], player.PlayerState.CardsPlayed);
		AssertPile([conspirator, throneRoom, conspirator, conspirator], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		user.Verify(u => u.ThroneRoomPlay(throneRoom, player.PlayerState,
			player.Game.Kingdom, It.IsAny<IEnumerable<CardInstance>>()), Times.Once);
		#endregion
	}
}
