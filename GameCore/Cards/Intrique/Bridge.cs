namespace GameCore.Cards.Intrique;

public class Bridge : Card
{
	private static Bridge bridge;
	private Bridge() : base
	(
		name: "Bridge",
		type: CardType.Bridge,
		price: 4,
		addActions: 0,
		addBuys: 1,
		addCoins: 1,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		bridge = this;
		Description = "This turn, cards (everywhere) cost $1 less.";
	}

	public static Bridge Get() => bridge ?? new Bridge();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.TempEffects.ReduceCost(1);
	}
}
