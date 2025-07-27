using System.Numerics;
using GameCore.Cards.GeneralCards;
using GameCoreTests.Cards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class AdventurerTests : CardTestsBase
{
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();
	private readonly Card province = Province.Get();
	private readonly Card adventurer = Adventurer.Get();

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
		player.SetupSequence(p => p.Show(It.IsAny<int>()))
			.Returns(new List<Card> { copper })
			.Returns(new List<Card> { silver });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows two cards
		player.Verify(p => p.Show(1), Times.Exactly(2));

		// player has the two treasures in his hand
		CollectionAssert.AreEqual(new List<Card> { copper, silver }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void SkipNonTreasures()
	{
		#region arrange
		player.SetupSequence(p => p.Show(It.IsAny<int>()))
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { adventurer })
			.Returns(new List<Card> { silver })
			.Returns(new List<Card> { gold });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows six cards
		player.Verify(p => p.Show(1), Times.Exactly(6));

		// player has the two treasures in his hand
		CollectionAssert.AreEqual(new List<Card> { silver, gold }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void OneTreasureToDraw()
	{
		#region arrange
		player.SetupSequence(p => p.Show(It.IsAny<int>()))
			.Returns(new List<Card> { province })
			.Returns(new List<Card> { gold })
			.Returns(new List<Card> { null });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player shows two cards (in the last call he does not have any)
		player.Verify(p => p.Show(1), Times.Exactly(3));

		// player has the treasure in his hand
		CollectionAssert.AreEqual(new List<Card> { gold }, player.Object.PlayerState.Hand);
		#endregion
	}

	[TestMethod]
	public void NoTreasuresToDraw()
	{
		#region arrange
		player.SetupSequence(p => p.Show(It.IsAny<int>()))
			.Returns(new List<Card> { null });
		#endregion

		#region act
		adventurer.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// player does not have any cards to draw
		player.Verify(p => p.Show(1), Times.Once);

		// player has the treasure in his hand
		CollectionAssert.AreEqual(new List<Card> { }, player.Object.PlayerState.Hand);
		#endregion
	}
}