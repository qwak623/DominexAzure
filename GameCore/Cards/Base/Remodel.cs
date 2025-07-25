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

	protected override void ActionEffect(IPlayer p)
	{
		// if user didnt select card he wont gain any.
		var oldCard = p.User.RemodelTrash(this, p.PlayerState, p.Game.Kingdom);
		if (oldCard == null)
		{
			return;
		}

		p.Trash(oldCard);

		var newCard = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(oldCard.Price + 2), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (newCard != null)
		{
			p.Gain(newCard.Type);
		}
	}
}
