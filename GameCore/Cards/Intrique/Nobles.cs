namespace GameCore.Cards.Intrique;
public class Nobles : Card
{
	private static Nobles nobles;
	private Nobles() : base([CardType.Action, CardType.Victory])
	{
		Name = CardName.Nobles;
		DefaultPrice = 6;
		nobles = this;
		VictoryPoints = 2;
	}

	public static Nobles Get() => nobles ?? new Nobles();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.User.NoblesChooseCards(this, player.PlayerState, player.Game.Kingdom))
		{
			player.Draw(3);
		}
		else
		{
			player.PlayerState.Actions += 2;
		}
	}
}
