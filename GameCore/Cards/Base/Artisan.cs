namespace GameCore.Cards.Base;
public class Artisan : Card
{
	private static Artisan artisan;
	private Artisan() : base(CardType.Action)
	{
		Name = CardName.Artisan;
		DefaultPrice = 6;
		artisan = this;
		Description = $"Gain a card to your hand costing up to $5." +
			$"{Environment.NewLine}Put a card from your hand onto your deck.";
	}

	public static Artisan Get() => artisan ?? new Artisan();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var availableCards = p.Game.Kingdom.GetWrapper(p.PlayerState, 5).AvailableCards.ToList();
		var cardToGain = p.User.SelectCardToGain(this, p.PlayerState, p.Game.Kingdom, availableCards);
		p.GainToHand(cardToGain);

		var card = p.User.ArtisanPutOnTop(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		p.ReturnToDrawPile(card);
	}
}
