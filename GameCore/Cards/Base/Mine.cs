namespace GameCore.Cards.Base;
public class Mine : Card
{
	private static Mine mine;
	private Mine() : base
	(
		name: "Mine",
		type: CardType.Mine,
		price: 5,
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
		mine = this;
		Description = "You may trash a Treasure card from your hand. Gain a Treasure card to your hand costing up to $3 more than it.";
		Message = "Trash a treasure, gain a treasure to your hand costing up to $3 more.";
	}

	public static Mine Get() => mine ?? new Mine(); // todo tohle neni thread safe - je potreba aby bylo?

	protected override void ActionEffect(IPlayer p)
	{
		var cardSelection = p.PlayerState.Hand.Where(c => c.IsTreasure).ToList();

		var oldCard = p.User.MineTrash(this, p.PlayerState, p.Game.Kingdom, cardSelection);
		if (oldCard == null)
		{
			return;
		}

		p.Trash(oldCard);
		var newCard = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(oldCard.Price + 3, true), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (newCard != null)
		{
			p.GainToHand(newCard.Type);
		}
	}
}
