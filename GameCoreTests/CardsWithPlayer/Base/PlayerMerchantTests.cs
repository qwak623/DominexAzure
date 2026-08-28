using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerMerchantTests : CardWithPlayerTestsBase
{
	private readonly Card merchant = Merchant.Get();
	private readonly Card estate = Estate.Get();
	private readonly Card silver = Silver.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame(merchant);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void FirstSilverPlayedAfterMerchantGetsTheBonusCoin()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([merchant, silver]);
		player.PlayerState.DrawPile = CreatePile([estate]);
		var merchantToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Merchant);
		#endregion

		#region act
		player.PlayActionCardInternal(merchantToPlay);
		player.PlayTreasure();
		#endregion

		#region assert
		// +1 Action cancels out playing merchant itself; silver's own $2 plus merchant's +$1
		AssertNumbers(1, 3, 0, player);
		AssertPile([estate, silver], player.PlayerState.Hand);
		AssertPile([merchant], player.PlayerState.CardsPlayed);
		AssertPile([merchant], player.PlayerState.ActionsPlayed);

		// the bonus is consumed by the first silver, not left to leak into a later turn
		Assert.AreEqual(0, player.PlayerState.TempEffects.FirstSilverValueIncrease);
		#endregion
	}

	[TestMethod]
	public void OnlyTheFirstSilverThisTurnGetsTheBonus()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([merchant, silver, silver]);
		player.PlayerState.DrawPile = CreatePile([estate]);
		var merchantToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Merchant);
		#endregion

		#region act
		player.PlayActionCardInternal(merchantToPlay);
		player.PlayTreasure();
		#endregion

		#region assert
		// first silver: $2 + $1 bonus; second silver: $2 with no bonus left = $5 total
		AssertNumbers(1, 5, 0, player);
		#endregion
	}

	[TestMethod]
	public void PlayingTwoMerchantsStacksTheBonusOntoTheSameFirstSilver()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([merchant, merchant, silver]);
		player.PlayerState.DrawPile = CreatePile([estate, estate]);
		var merchantsToPlay = player.PlayerState.Hand.Where(c => c.Card.Name == CardName.Merchant).ToList();
		#endregion

		#region act
		player.PlayActionCardInternal(merchantsToPlay[0]);
		player.PlayActionCardInternal(merchantsToPlay[1]);
		player.PlayTreasure();
		#endregion

		#region assert
		// both merchants' bonuses land on the single silver played this turn: $2 + $1 + $1 = $4
		AssertNumbers(1, 4, 0, player);
		Assert.AreEqual(0, player.PlayerState.TempEffects.FirstSilverValueIncrease);
		#endregion
	}

	[TestMethod]
	public void SilverGetsNoBonusWithoutMerchant()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([silver]);
		#endregion

		#region act
		player.PlayTreasure();
		#endregion

		#region assert
		AssertNumbers(1, 2, 0, player);
		#endregion
	}
}
