namespace GameCore.Cards.Intrique;

public class Conspirator : Card
{
	private static Conspirator conspirator;
	private Conspirator() : base
	(
		type: CardName.Conspirator,
		price: 4,
		addActions: 0,
		addBuys: 0,
		addCoins: 2,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		conspirator = this;
		Description = "If you've played 3 or more Actions this turn (counting this), +1 Card and +1 Action.";
	}

	public static Conspirator Get() => conspirator ?? new Conspirator();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.ActionsPlayed.Count >= 3)
		{
			player.Draw(1);
			player.PlayerState.Actions += 1;
		}
	}
}
