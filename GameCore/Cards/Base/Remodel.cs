namespace GameCore.Cards.Base;
public class Remodel : Card
{
	private static Remodel remodel;
	private Remodel() : base
	(
		name: "Remodel",
		type: CardType.Remodel,
		price: 4,
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
		remodel = this;
		Description = "Trash a card from your hand. Gain a card costing up to $2 more than it.";
	}

	public static Remodel Get() => remodel ?? new Remodel();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		// if user didn't select card, he wouldn't gain any.
		var oldCard = p.User.RemodelTrash(this, p.PlayerState, p.Game.Kingdom, p.PlayerState.Hand.ToList());
		if (oldCard is null)
		{
			return;
		}

		p.Trash(oldCard);

		// todo rethink how to get old card price (can be influenced by bridge, etc.)
		var newCard = p.User.SelectCardToGain(
			p.Game.Kingdom.GetWrapper(p.PlayerState, oldCard.Card.GetPrice(p.PlayerState) + 2), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (newCard is not null)
		{
			p.Gain(newCard.Card.Type);
		}
	}
}
