namespace GameCore.Cards.Intrique;
public class Upgrade : Card
{
	private static Upgrade upgrade;
	private Upgrade() : base
	(
		type: CardName.Upgrade,
		price: 5,
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
		upgrade = this;
		Description = "Trash a card from your hand. Gain a card costing exactly $1 more than it.";
	}

	public static Upgrade Get() => upgrade ?? new Upgrade();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		if (p.PlayerState.Hand.Count == 0)
		{
			return;
		}

		var oldCard = p.User.UpgradeTrash(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		if (oldCard is null)
		{
			return;
		}

		p.Trash(oldCard);

		var price = oldCard.Card.GetPrice(p.PlayerState) + 1;
		var newCard = p.User.SelectCardToGain(
			new KingdomWrapper() { Kingdom = p.Game.Kingdom, MinPrice = price, MaxPrice = price, PlayerState = p.PlayerState }, p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (newCard is not null)
		{
			p.Gain(newCard);
		}
	}
}
