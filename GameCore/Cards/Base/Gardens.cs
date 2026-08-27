namespace GameCore.Cards.Base;
public class Gardens : Card
{
	private static Gardens gardens;
	private Gardens() : base(CardType.Victory)
	{
		Name = CardName.Gardens;
		DefaultPrice = 4;
		gardens = this;
		Description = "Worth 1 VP per 10 cards you have (round down).";
	}

	public static Gardens Get() => gardens ?? new Gardens();

	public override int CountPoints(IPlayer player) => player.PlayerState.DiscardPile.Count / 10;
}
