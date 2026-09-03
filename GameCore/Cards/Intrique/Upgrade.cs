namespace GameCore.Cards.Intrique;
public class Upgrade : Card
{
	private static Upgrade upgrade;
	private Upgrade() : base(CardType.Action)
	{
		Name = CardName.Upgrade;
		DefaultPrice = 5;
		AddActions = 1;
		DrawCards = 1;
		upgrade = this;
		Description = "Trash a card from your hand. Gain a card costing exactly $1 more than it.";
	}

	public static Upgrade Get() => upgrade ?? new Upgrade();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var oldCard = p.User.UpgradeTrash(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		if (oldCard is null)
		{
			return;
		}

		p.Trash(oldCard);

		var price = oldCard.Card.GetPrice(p.PlayerState) + 1;
		var availableCards = new KingdomWrapper()
		{ Kingdom = p.Game.Kingdom, MinPrice = price, MaxPrice = price, PlayerState = p.PlayerState }
			.AvailableCards.ToList();
		var newCard = p.User.SelectCardToGain(this, p.PlayerState, p.Game.Kingdom, availableCards);
		p.Gain(newCard);
	}
}
