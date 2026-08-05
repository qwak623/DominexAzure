namespace GameCore.Cards.Intrique;

public class ShantyTown : Card
{
	private static ShantyTown shantyTown;
	private ShantyTown() : base
	(
		name: "Shanty Town",
		type: CardType.ShantyTown,
		price: 3,
		addActions: 2,
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
		shantyTown = this;
		Description = $"Reveal your hand. If you have no Action cads in hand, +2 Card.";
	}

	public static ShantyTown Get() => shantyTown ?? new ShantyTown();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		// TODO reveal your hand
		if (player.PlayerState.Hand.All(c => !c.Card.IsAction))
		{
			player.Draw(2);
		}
	}
}
