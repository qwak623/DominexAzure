namespace GameCore.Cards.Intrique;
public class WishingWell : Card
{
	private static WishingWell wishingWell;
	private WishingWell() : base(CardType.Action)
	{
		Name = CardName.WishingWell;
		DefaultPrice = 3;
		AddActions = 1;
		DrawCards = 1;
		wishingWell = this;
		Description = "Name a card, then reveal the top card of your deck. If you named it, put it into your hand.";
	}

	public static WishingWell Get() => wishingWell ?? new WishingWell();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		CardName guessedCard = p.User.WishingWellGuess(this, p.PlayerState, p.Game.Kingdom, Enum.GetValues<CardName>().ToList());
		CardInstance topCard = p.Show(1).FirstOrDefault();
		if (topCard?.Card.Name != guessedCard)
		{
			if (topCard is not null)
			{
				p.ReturnToDrawPile(topCard);
			}
			return;
		}
		p.PlayerState.Hand.Move(topCard);
	}
}
