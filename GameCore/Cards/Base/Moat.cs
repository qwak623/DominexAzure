namespace GameCore.Cards.Base;

public class Moat : Card
{
	private static Moat moat;
	private Moat() : base([CardType.Action, CardType.Reaction])
	{
		Name = CardName.Moat;
		DefaultPrice = 2;
		DrawCards = 2;
		moat = this;
		Description = "When another player plays an Attack card, you may first reveal this from your hand, to be unaffected by it.";
	}

	public static Moat Get() => moat ?? new Moat();

	public override bool Reaction(IPlayer player) => true;
}
