namespace GameCore.Cards.Intrique;
public class Nobles : Card
{
	private static Nobles nobles;
	private Nobles() : base
	(
		type: CardName.Nobles,
		price: 6,
		addActions: 0,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: true,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
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
