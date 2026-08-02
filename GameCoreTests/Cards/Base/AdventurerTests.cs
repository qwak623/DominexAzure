#if false
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class AdventurerTests : CardTestsBase
{
	private readonly Card adventurer = Adventurer.Get();
	private readonly Card throneRoom = ThroneRoom.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card province = Province.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(adventurer));
	}

	[TestMethod]
	public void DrawTwoTreasures()
	{
		#region arrange
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { silver });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows two cards
		player.Verify(p => p.Show(1), Times.Exactly(2));

		// nothing was discarded
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void SkipNonTreasures()
	{
		#region arrange
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { silver })
			.Returns(new List<Card> { gold });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows six cards
		player.Verify(p => p.Show(1), Times.Exactly(6));

		// the non-treasure cards were discarded
		player.Verify(p => p.Discard(adventurer), Times.Exactly(3));
		player.Verify(p => p.Discard(province), Times.Once);

		// player has the two treasures in his hand
		CollectionAssert.AreEquivalent(new List<Card> { silver, gold }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void OneTreasureToDraw()
	{
		#region arrange
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { gold })
			.Returns(new List<Card> { null });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows two cards (in the last call he does not have any)
		player.Verify(p => p.Show(1), Times.Exactly(3));

		// the non-treasure card was discarded
		player.Verify(p => p.Discard(province), Times.Once);

		// player has the treasure in his hand
		CollectionAssert.AreEquivalent(new List<Card> { gold }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void NoTreasuresToDraw()
	{
		#region arrange
		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { null });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object, TODO);
		#endregion

		#region assert
		// +0 Actions, +0 Coins, +0 Buys 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// +0 Cards
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player does not have any cards to draw
		player.Verify(p => p.Show(1), Times.Once);

		// nothing was discarded
		player.Verify(p => p.Discard(It.IsAny<Card>()), Times.Never);

		// player has nothing in his hand
		CollectionAssert.AreEquivalent(new List<Card> { }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void ThroneRoom4Treasures()
	{
		#region arrange
		player.Object.PlayerState.Hand = new List<Card> { adventurer };

		player.SetupSequence(p => p.Show(1))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { silver })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { gold })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { copper });

		player.Setup(p => p.User.ThroneRoomPlay(throneRoom, player.Object.PlayerState,
			player.Object.Game.Kingdom, It.Is<IEnumerable<Card>>(c => c.Contains(adventurer)))).Returns(adventurer);
		#endregion

		#region act
		throneRoom.WhenPlayAction(player.Object, TODO);
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

		// player shows 9 cards
		player.Verify(p => p.Show(1), Times.Exactly(9));

		// the non treasure cards were discarded
		player.Verify(p => p.Discard(adventurer), Times.Once);
		player.Verify(p => p.Discard(province), Times.Exactly(4));

		// the hand should now have the four treasures
		CollectionAssert.AreEquivalent(new List<Card> { copper, silver, gold, copper }, player.Object.PlayerState.Hand);
		#endregion
	}
}
#endif
