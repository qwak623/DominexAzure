namespace GameCore.Cards.Base;

public class Witch : Card
{
	private static Witch witch;
	private Witch() : base
	(
		type: CardName.Witch,
		price: 5,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 2,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true
	)
	{
		witch = this;
		Description = "Each other player gains a Curse.";
	}

	public static Witch Get() => witch ?? new Witch();

	public override Card RequiredCards => GeneralCards.Curse.Get();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker) => defender.Gain(CardName.Curse);
}
