namespace GameCore.Cards.Base;
public class Workshop : Card
{
	private static Workshop workshop;
	private Workshop() : base
	(
		name: "Workshop",
		type: CardType.Workshop,
		price: 3,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		workshop = this;
		Description = "Gain a card costing up to $4.";
	}

	public static Workshop Get() => workshop ?? new Workshop();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var card = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(4), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (card is not null)
		{
			p.Gain(card);
		}
	}
}
