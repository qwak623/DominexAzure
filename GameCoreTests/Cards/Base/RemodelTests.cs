using GameCore.Cards.GeneralCards;
using GameCore.Cards.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GameCore.Cards.Base.Tests;

[TestClass]
public class RemodelTests : CardTestsBase
{
	private readonly Card remodel = Remodel.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card laboratory = Laboratory.Get();

	private Mock<IPlayer> player;

	[TestInitialize]
	public void Init()
	{
		player = MockPlayer(MockKingdom(laboratory));
		player.Object.PlayerState.Hand = new List<Card> { silver, laboratory };
	}

	[TestMethod]
	public void UpgradeSilverToLaboratory()
	{
		#region arrange
		player.Setup(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(silver);
		player.Setup(p => p.User.SelectCardToGain(
				It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
				player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns(laboratory);
		#endregion

		#region act
		remodel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a card to trash
		player.Verify(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(silver), Times.Once);

		// user is asked to select a card with max price 5 to gain
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 5 && !kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// laboratory is gained
		player.Verify(p => p.Gain(CardType.Laboratory), Times.Once);
		#endregion
	}

	[TestMethod]
	public void DontTrashAnything()
	{
		#region arrange
		player.Setup(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, player.Object.Game.Kingdom)).Returns<Card>(null);
		#endregion

		#region act
		remodel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a card to trash
		player.Verify(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, player.Object.Game.Kingdom), Times.Once);

		// player does not trash anything
		player.Verify(p => p.Trash(It.IsAny<Card>()), Times.Never);

		// user is never asked to choose any card to gain
		player.Verify(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(),
			It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<Phase>()), Times.Never);

		// nothing is gained
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}

	[TestMethod]
	public void DontGainAnything()
	{
		#region arrange
		player.Setup(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, player.Object.Game.Kingdom))
			.Returns(laboratory);
		player.Setup(p => p.User.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain))
			.Returns<Card>(null);
		#endregion

		#region act
		remodel.WhenPlayAction(player.Object);
		#endregion

		#region assert
		// actions, coins and buys shouldn't change 
		Assert.AreEqual(0, player.Object.PlayerState.Actions);
		Assert.AreEqual(0, player.Object.PlayerState.Coins);
		Assert.AreEqual(0, player.Object.PlayerState.Buys);

		// player does not draw a card
		player.Verify(p => p.Draw(It.IsAny<int>()), Times.Never);

		// user is asked to choose a card to trash
		player.Verify(p => p.User.RemodelTrash(remodel, player.Object.PlayerState, It.IsAny<Kingdom>()), Times.Once);

		// player trashes the chosen card
		player.Verify(p => p.Trash(laboratory), Times.Once);

		// user is asked to select a card with max price 7 to gain, but there is no such card available
		player.Verify(p => p.User.SelectCardToGain(It.Is<KingdomWrapper>(kw => kw.Price == 7 && !kw.OnlyTreasures),
			player.Object.PlayerState, player.Object.Game.Kingdom, Phase.Gain), Times.Once);

		// nothing is gained
		player.Verify(p => p.Gain(It.IsAny<CardType>()), Times.Never);
		#endregion
	}
}