namespace GameCore.Cards.GeneralCards;
public class Province : Card
{
	private static Province province;
	private Province() : base(CardType.Victory)
	{
		Name = CardName.Province;
		DefaultPrice = 8;
		province = this;
		VictoryPoints = 6;
	}

	public static Province Get() => province ?? new Province();
}
