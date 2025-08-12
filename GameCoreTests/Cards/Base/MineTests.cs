using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class MineTests : CardTestsBase
{
	private readonly Card mine = Mine.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(mine));
		player.Object.PlayerState.Hand = new List<Card> { copper };
	}

	[TestMethod]
	public void UpgradeCopperToSilver()
	{
		#region arrange
		player.Setup(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns(copper);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(silver);
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(copper), Times.Once);

		// user is asked to select a treasure with max price 3 to gain (it's silver)
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// silver is added to the hand
		player.Verify(p => p.GainToHand(CardType.Silver), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.Setup(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns<Card>(null);
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom,
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player does not trash anything
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);

		// user is never asked to choose any card to gain
		player.Verify(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);

		// nothing is gained
		player.Verify(p => p.GainToHand(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.Setup(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns(copper);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		mine.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a treasure to trash
		player.Verify(p => p.User.MineTrash(mine, player.Object.PlayerState, It.IsAny<Kingdom>(),
			It.Is<IList<Card>>(c => c.SequenceEqual(new List<Card> { copper }))), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(copper), Times.Once);

		// user is asked to select a treasure with max price 3 to gain, but there is no such card available
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// nothing is added to the hand
		player.Verify(p => p.GainToHand(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void ThroneRoomPlay()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { mine, copper };
		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.SingleOrDefault() == mine))).Returns(mine);
		player.SetupSequence(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom, It.IsAny<IList<Card>>()))
			.Returns(copper).Returns(silver);
		player.SetupSequence(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(silver).Returns(gold);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked which card to play using throne room
		player.Verify(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.IsAny<IEnumerable<Card>>()), Times.Once);

		// user is asked to choose a treasure to trash two times
		player.Verify(p => p.User.MineTrash(mine, player.Object.PlayerState, player.Object.Game.Kingdom,
			It.IsAny<IList<Card>>()), Times.Exactly(2));

		// player trashes the chosen cards
		player.Verify(p => p.Trash(copper), Times.Once);
		player.Verify(p => p.Trash(silver), Times.Once);

		// user is asked to select a treasure with max price 3 or 6 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 3 && kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 6 && kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// silver and gold are added to the hand
		player.Verify(p => p.GainToHand(CardType.Silver), Times.Once);
		player.Verify(p => p.GainToHand(CardType.Gold), Times.Once);
		#endregion
	}
}