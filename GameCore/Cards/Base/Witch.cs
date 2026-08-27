namespace GameCore.Cards.Base;

public class Witch : Card
{
	private static Witch witch;
	private Witch() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Witch;
		DefaultPrice = 5;
		DrawCards = 2;
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
