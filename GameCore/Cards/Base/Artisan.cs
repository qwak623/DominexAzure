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
		var cardToGain = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(p.PlayerState, 5), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (cardToGain is not null)
		{
			p.GainToHand(cardToGain);
		}

		if (p.PlayerState.Hand.Count == 0)
		{
			return;
		}
		var card = p.User.ArtisanPutOnTop(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		p.ReturnToDrawPile(card);
	}
}
