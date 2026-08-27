namespace GameCore.Cards.Intrique;

public class MiningVillage : Card
{
	private static MiningVillage miningVillage;
	private MiningVillage() : base(CardType.Action)
	{
		Name = CardName.MiningVillage;
		DefaultPrice = 4;
		AddActions = 2;
		DrawCards = 1;
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
