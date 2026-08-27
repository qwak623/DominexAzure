using GameCore.Cards;
using GameCore.Cards.GeneralCards;
using GameCore.Cards.Intrique;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Intrique.Tests;

[TestClass]
public class PlayerCoppersmithTests : CardWithPlayerTestsBase
{
	private readonly Card coppersmith = Coppersmith.Get();
	private readonly Card copper = Copper.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card gold = Gold.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(coppersmith);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void PlayCoppersmith()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([coppersmith]);
		var coppersmithToPlay = player.PlayerState.Hand[0];
		#endregion

		#region act
		player.PlayActionCardInternal(coppersmithToPlay);
		#endregion

		#region assert
		// coppersmith has no inherent actions/coins/buys of its own
		AssertNumbers(0, 0, 0, player);
		Assert.AreEqual(1, player.PlayerState.TempEffects.CopperValueIncrease);

		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([coppersmith], player.PlayerState.CardsPlayed);
		AssertPile([coppersmith], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void IncreasesCopperValueWhenPlayed()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([coppersmith, copper]);
		var coppersmithToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Coppersmith);
		#endregion

		#region act
		player.PlayActionCardInternal(coppersmithToPlay);
		player.PlayTreasure();
		#endregion

		#region assert
		// coppersmith itself adds nothing; copper: base $1 + coppersmith's +1 bonus = $2
		AssertNumbers(0, 2, 0, player);
		// treasures aren't moved out of hand when "played" (see Player.PlayTreasure) - they
		// only move to discard at cleanup, same as every other treasure in this codebase
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([coppersmith], player.PlayerState.CardsPlayed);
		AssertPile([coppersmith], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void StacksWithMultipleCoppersmiths()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([coppersmith, coppersmith, copper]);
		var coppersmithsToPlay = player.PlayerState.Hand.Where(c => c.Card.Name == CardName.Coppersmith).ToList();
		#endregion

		#region act
		foreach (var coppersmithToPlay in coppersmithsToPlay)
		{
			player.PlayActionCardInternal(coppersmithToPlay);
		}
		player.PlayTreasure();
		#endregion

		#region assert
		// coppersmiths add nothing themselves; copper: base $1 + $2 from the two coppersmiths = $3
		AssertNumbers(-1, 3, 0, player);
		Assert.AreEqual(2, player.PlayerState.TempEffects.CopperValueIncrease);
		AssertPile([copper], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([coppersmith, coppersmith], player.PlayerState.CardsPlayed);
		AssertPile([coppersmith, coppersmith], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void DoesNotAffectSilverOrGold()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([coppersmith, silver, gold]);
		var coppersmithToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Coppersmith);
		#endregion

		#region act
		player.PlayActionCardInternal(coppersmithToPlay);
		player.PlayTreasure();
		#endregion

		#region assert
		// silver ($2) and gold ($3) are unaffected by coppersmith's copper-only bonus
		AssertNumbers(0, 5, 0, player);
		AssertPile([silver, gold], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([coppersmith], player.PlayerState.CardsPlayed);
		AssertPile([coppersmith], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);
		#endregion
	}

	[TestMethod]
	public void ResetsOnCleanup()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([coppersmith]);
		var coppersmithToPlay = player.PlayerState.Hand[0];
		player.PlayActionCardInternal(coppersmithToPlay);
		#endregion

		#region act
		player.Cleanup();
		#endregion

		#region assert
		Assert.AreEqual(0, player.PlayerState.TempEffects.CopperValueIncrease);
		#endregion
	}
}
