namespace GameCore.Cards.Intrique;
public class WishingWell : Card
{
	private static WishingWell wishingWell;
	private WishingWell() : base
	(
		type: CardName.WishingWell,
		price: 3,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 1,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
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
