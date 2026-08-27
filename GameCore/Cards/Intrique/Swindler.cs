namespace GameCore.Cards.Intrique;
public class Swindler : Card
{
	private static Swindler swindler;
	private Swindler() : base
	(
		type: CardName.Swindler,
		price: 3,
		addActions: 0,
		addBuys: 0,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true
	)
	{
		swindler = this;
		Description = "Each other player trashes the top card of their deck and gains a card with the same cost that you choose.";
	}

	public static Swindler Get() => swindler ?? new Swindler();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		var cardToTrash = defender.Show(1).SingleOrDefault();
		if (cardToTrash == null)
		{
			return;
		}

		defender.Trash(cardToTrash);
		var price = cardToTrash.Card.GetPrice(defender.PlayerState);
		var cardToGain = attacker.User.SelectCardToGain(
			new KingdomWrapper() { Kingdom = defender.Game.Kingdom, MinPrice = price, MaxPrice = price, PlayerState = defender.PlayerState },
			defender.PlayerState, defender.Game.Kingdom, Phase.Action);
		if (cardToGain != null)
		{
			defender.Gain(cardToGain);
		}
	}
}
