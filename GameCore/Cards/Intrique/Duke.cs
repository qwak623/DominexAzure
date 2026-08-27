namespace GameCore.Cards.Intrique;
public class Duke : Card
{
	private static Duke duke;
	private Duke() : base
	(
		type: CardName.Duke,
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
		Description = "Worth 1 VP per Duchy you have.";
	}

	public static Duke Get() => duke ?? new Duke();

	public override int CountPoints(IPlayer player) => player.PlayerState.DiscardPile.Count(c => c.Card.Name == CardName.Duchy);
}
