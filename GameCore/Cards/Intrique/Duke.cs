namespace GameCore.Cards.Intrique;
public class Duke : Card
{
	private static Duke duke;
	private Duke() : base
	(
		name: "Duke",
		type: CardType.Duke,
		price: 5,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: true,
		isTreasure: false,
		isAction: false,
		isReaction: false,
		isAttack: false
	)
	{
		duke = this;
		Description = "Worth 1 VP per 10 cards you have (round down).";
	}

	public static Duke Get() => duke ?? new Duke();

	public override int CountPoints(IPlayer player) => player.PlayerState.DiscardPile.Count(c => c.Card.Type == CardType.Duchy);
}
