namespace GameCore.Cards.Intrique;

public class ShantyTown : Card
{
	private static ShantyTown shantyTown;
	private ShantyTown() : base(CardType.Action)
	{
		Name = CardName.ShantyTown;
		DefaultPrice = 3;
		AddActions = 2;
		shantyTown = this;
		Description = $"Reveal your hand. If you have no Action cads in hand, +2 Card.";
	}

	public static ShantyTown Get() => shantyTown ?? new ShantyTown();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		// TODO reveal your hand
		if (player.PlayerState.Hand.All(c => !c.Card.IsAction))
		{
			player.Draw(2);
		}
	}
}
