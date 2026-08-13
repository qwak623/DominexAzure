namespace GameCore.Cards.Intrique;

public class Coppersmith : Card
{
	private static Coppersmith coppersmith;
	private Coppersmith() : base
	(
		name: "Coppersmith",
		type: CardType.Coppersmith,
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
		coppersmith = this;
		Description = "Copper produces an extra $1 this turn.";
	}

	public static Coppersmith Get() => coppersmith ?? new Coppersmith();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.TempEffects.IncreaseCopperValue(1);
	}
}
