namespace GameCore.Cards.Intrique;
public class GreatHall : Card
{
	private static GreatHall greatHall;
	private GreatHall() : base([CardType.Victory, CardType.Action])
	{
		Name = CardName.GreatHall;
		DefaultPrice = 3;
		AddActions = 1;
		DrawCards = 1;
		greatHall = this;
		VictoryPoints = 1;
	}

	public static GreatHall Get() => greatHall ?? new GreatHall();
}
