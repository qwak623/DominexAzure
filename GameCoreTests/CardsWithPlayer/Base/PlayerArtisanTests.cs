using GameCore.Cards;
using GameCore.Cards.Base;
using GameCore.Cards.GeneralCards;
using GameCore.CardWithPlayer.Tests;
using Moq;

namespace GameCore.CardsWithPlayer.Base.Tests;

[TestClass]
public class PlayerArtisanTests : CardWithPlayerTestsBase
{
	private readonly Card artisan = Artisan.Get();
	private readonly Card silver = Silver.Get();
	private readonly Card laboratory = Laboratory.Get();

	private Player player;

	private Mock<IUser> user;
	private Mock<IGame> game;

	[TestInitialize]
	public void Init()
	{
		game = MockGame([artisan, laboratory]);
		user = new Mock<IUser>();
		player = CreatePlayer(game.Object, user.Object);
	}

	[TestMethod]
	public void GainsCardToHandAndPutsChosenCardOnTopOfDeck()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([artisan, silver]);
		var artisanToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Artisan);

		KingdomWrapper wrapper = null;
		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Callback<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => wrapper = kw)
			.Returns(() => wrapper.GetCard(CardName.Laboratory));

		// silver (already in hand) is put back, keeping the newly gained laboratory in hand
		user.Setup(u => u.ArtisanPutOnTop(artisan, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 2))).Returns<Card, PlayerState, Kingdom, List<CardInstance>>(
			(c, ps, k, cards) => cards.Single(x => x.Card.Name == CardName.Silver));
		#endregion

		#region act
		player.PlayActionCardInternal(artisanToPlay);
		#endregion

		#region assert
		AssertNumbers(0, 0, 0, player);
		AssertPile([laboratory], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		AssertPile([artisan], player.PlayerState.CardsPlayed);
		AssertPile([artisan], player.PlayerState.ActionsPlayed);
		AssertPile([], player.Game.Trash);

		// gaining is capped at $5, not "up to $5 or more"
		Assert.AreEqual(5, wrapper.MaxPrice);
		user.Verify(u => u.ArtisanPutOnTop(artisan, player.PlayerState, player.Game.Kingdom, It.IsAny<List<CardInstance>>()), Times.Once);
		#endregion
	}

	[TestMethod]
	public void TheJustGainedCardCanBePutStraightBackOnTheDeck()
	{
		#region arrange
		// hand has nothing but artisan itself before the gain
		player.PlayerState.Hand = CreatePile([artisan]);
		var artisanToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns<KingdomWrapper, PlayerState, Kingdom, Phase>((kw, ps, k, p) => kw.GetCard(CardName.Laboratory));
		user.Setup(u => u.ArtisanPutOnTop(artisan, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 1))).Returns<Card, PlayerState, Kingdom, List<CardInstance>>(
			(c, ps, k, cards) => cards.Single());
		#endregion

		#region act
		player.PlayActionCardInternal(artisanToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([laboratory], player.PlayerState.DrawPile);
		#endregion
	}

	[TestMethod]
	public void PutsBackACardEvenWhenNothingWasGained()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([artisan, silver]);
		var artisanToPlay = player.PlayerState.Hand.First(c => c.Card.Name == CardName.Artisan);

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
		user.Setup(u => u.ArtisanPutOnTop(artisan, player.PlayerState, player.Game.Kingdom,
			It.Is<List<CardInstance>>(c => c.Count == 1))).Returns<Card, PlayerState, Kingdom, List<CardInstance>>(
			(c, ps, k, cards) => cards.Single());
		#endregion

		#region act
		player.PlayActionCardInternal(artisanToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([silver], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);
		#endregion
	}

	[TestMethod]
	public void NothingToPutBackWhenHandIsEmptyAndNothingWasGained()
	{
		#region arrange
		player.PlayerState.Hand = CreatePile([artisan]);
		var artisanToPlay = player.PlayerState.Hand[0];

		user.Setup(u => u.SelectCardToGain(It.IsAny<KingdomWrapper>(), player.PlayerState, player.Game.Kingdom, Phase.Gain))
			.Returns((CardInstance)null);
		#endregion

		#region act
		player.PlayActionCardInternal(artisanToPlay);
		#endregion

		#region assert
		AssertPile([], player.PlayerState.Hand);
		AssertPile([], player.PlayerState.DrawPile);
		AssertPile([], player.PlayerState.DiscardPile);

		user.Verify(u => u.ArtisanPutOnTop(It.IsAny<Card>(), It.IsAny<PlayerState>(), It.IsAny<Kingdom>(), It.IsAny<List<CardInstance>>()), Times.Never);
		#endregion
	}
}
