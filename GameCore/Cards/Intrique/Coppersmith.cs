namespace GameCore.Cards.Intrique;

public class Coppersmith : Card
{
	private static Coppersmith coppersmith;
	private Coppersmith() : base(CardType.Action)
	{
		Name = CardName.Coppersmith;
		DefaultPrice = 4;
		coppersmith = this;
		Description = "Copper produces an extra $1 this turn.";
	}

	public static Coppersmith Get() => coppersmith ?? new Coppersmith();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.PlayerState.TempEffects.IncreaseCopperValue(1);
	}
}
