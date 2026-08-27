namespace GameCore.Cards.GeneralCards;

public class Curse : Card
{
	private static Curse curse;
	private Curse() : base(CardType.Curse)
	{
		Name = CardName.Curse;
		DefaultPrice = 0;
		curse = this;
		VictoryPoints = -1;
	}

	public static Curse Get() => curse ?? new Curse();
}
