namespace GameCore.Cards.Intrique;

public class MiningVillage : Card
{
	private static MiningVillage miningVillage;
	private MiningVillage() : base
	(
		type: CardName.MiningVillage,
		price: 4,
		addActions: 2,
		addBuys: 0,
		addCoins: 0,
		drawCards: 1,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		miningVillage = this;
		Description = $"You may trash this for +$2.";
	}

	public static MiningVillage Get() => miningVillage ?? new MiningVillage();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (thisCard.Pile != player.Game.Trash && player.User.MiningVillageTrash(this, player.PlayerState, player.Game.Kingdom, thisCard))
		{
			player.Trash(thisCard);
			player.PlayerState.Coins += 2;
		}
	}
}
