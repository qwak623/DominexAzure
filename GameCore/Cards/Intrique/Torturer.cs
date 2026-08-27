namespace GameCore.Cards.Intrique;
public class Torturer : Card
{
	private static Torturer torturer;
	private Torturer() : base([CardType.Action, CardType.Attack])
	{
		Name = CardName.Torturer;
		DefaultPrice = 5;
		DrawCards = 3;
		torturer = this;
		Description = "Each other player either discards 2 cards or gains a Curse to their hand, their choice. (They may pick an option they can't do.)";
	}

	public static Torturer Get() => torturer ?? new Torturer();

	public override Card RequiredCards => GeneralCards.Curse.Get();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		TriggerAttacks(player);
	}

	public override void Attack(IPlayer defender, IPlayer attacker)
	{
		if (defender.User.TorturerChooseCurse(this, defender.PlayerState, defender.Game.Kingdom))
		{
			defender.GainToHand(CardName.Curse);
		}
		else if (defender.PlayerState.Hand.Count != 0)
		{
			var cardsToDiscard = defender.User.TorturerDiscard(
				this, defender.PlayerState, defender.Game.Kingdom, defender.PlayerState.Hand.ToList(), Math.Min(2, defender.PlayerState.Hand.Count));
			cardsToDiscard.ForEach(defender.Discard);
		}
	}
}

