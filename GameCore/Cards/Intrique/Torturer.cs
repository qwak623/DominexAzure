namespace GameCore.Cards.Intrique;
public class Torturer : Card
{
	private static Torturer torturer;
	private Torturer() : base
	(
		name: "Torturer",
		type: CardType.Torturer,
		price: 5,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 3,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: true
	)
	{
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
			defender.GainToHand(CardType.Curse);
		}
		else if (defender.PlayerState.Hand.Count != 0)
		{
			var cardsToDiscard = defender.User.TorturerDiscard(
				this, defender.PlayerState, defender.Game.Kingdom, Math.Min(2, defender.PlayerState.Hand.Count));
			cardsToDiscard.ForEach(defender.Discard);
		}
	}
}

