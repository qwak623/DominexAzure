namespace GameCore.Cards.Base;
public class Village : Card
{
	private static Village village;
	private Village() : base(CardType.Action)
	{
		Name = CardName.Village;
		DefaultPrice = 3;
		AddActions = 2;
		DrawCards = 1;
		village = this;
	}

	public static Village Get() => village ?? new Village();
}
