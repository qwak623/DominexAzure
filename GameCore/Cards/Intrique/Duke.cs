namespace GameCore.Cards.Intrique;
public class Duke : Card
{
	private static Duke duke;
	private Duke() : base(CardType.Victory)
	{
		Name = CardName.Duke;
		DefaultPrice = 5;
		duke = this;
		Description = "Worth 1 VP per Duchy you have.";
	}

	public static Duke Get() => duke ?? new Duke();

	public override int CountPoints(IPlayer player) => player.PlayerState.DiscardPile.Count(c => c.Card.Name == CardName.Duchy);
}
