using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerVassalTests : CardWithPlayerTestsBase
{
	private readonly Card vassal = Vassal.Get();
	private readonly Card village = Village.Get();
	private readonly Card copper = Copper.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([vassal, village]);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void NothingHappensWhenDrawPileIsEmpty()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([vassal]);
		player.PlayerState.DrawPile = CreatePile([]);
		var vassalToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(vassalToPlay);
		#endregion

		#region assert
		// vassal grants no actions of its own, so playing it just spends the one action available;
		// +$2 comes from vassal's own effect
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([vassal], player.PlayerState.CardsPlayed);
		AssertPile([vassal], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.VassalPlay(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<CardInstance>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void NonActionCardIsDiscardedWithoutAsking()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([vassal]);
		player.PlayerState.DrawPile = CreatePile([copper]);
		var vassalToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(vassalToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([copper], player.PlayerState.DiscardPile);
		AssertPile([vassal], player.PlayerState.CardsPlayed);
		AssertPile([vassal], player.PlayerState.ActionsPlayed);

		user.Verify(u => u.VassalPlay(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<CardInstance>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void RevealedActionCardIsDiscardedWhenPlayerDeclinesToPlayIt()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([vassal]);
		player.PlayerState.DrawPile = CreatePile([village]);
		var vassalToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.VassalPlay(vassal, player.PlayerState, player.Game.Kingdom, It.IsAny<CardInstance>())).Returns(false);
		#endregion

		#region act
		player.PlayActionCardInternal(vassalToPlay);
		#endregion

		#region assert
		// village's own effect (+2 actions, +1 card) never triggers since it was declined
		AssertNumbers(0, 2, 0, player);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([village], player.PlayerState.DiscardPile);
		AssertPile([vassal], player.PlayerState.CardsPlayed);
		AssertPile([vassal], player.PlayerState.ActionsPlayed);
		#endregion
	}

	[TestMethod]
	public void RevealedActionCardIsPlayedWhenPlayerAcceptsIt()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([vassal]);
		// village is revealed by vassal and, once played, draws copper as its own +1 Card
		player.PlayerState.DrawPile = CreatePile([copper, village]);
		var vassalToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.VassalPlay(vassal, player.PlayerState, player.Game.Kingdom, It.IsAny<CardInstance>())).Returns(true);
		#endregion

		#region act
		player.PlayActionCardInternal(vassalToPlay);
		#endregion

		#region assert
		// vassal grants no actions itself, so playing it spends the one available action down to
		// 0; village is then played "for free" (not through the normal action-spending path) and
		// adds its own +2 actions; +$2 comes from vassal's own effect
		AssertNumbers(2, 2, 0, player);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([vassal, village], player.PlayerState.CardsPlayed);
		AssertPile([vassal, village], player.PlayerState.ActionsPlayed);
		#endregion
	}
}
