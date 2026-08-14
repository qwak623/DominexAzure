namespace GameCore.Cards.Intrique;
public class Lurker : Card
{
	private static Lurker lurker;
	private Lurker() : base
	(
		name: "Lurker",
		type: CardType.Lurker,
		price: 2,
		addActions: 1,
		addBuys: 0,
		addCoins: 0,
		drawCards: 0,
		isVictory: false,
		isTreasure: false,
		isAction: true,
		isReaction: false,
		isAttack: false
	)
	{
		lurker = this;
		Description = "Choose one: Trash an Action card from the Supply; or gain an Action card from the trash.";
	}

	public static Lurker Get() => lurker ?? new Lurker();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.User.LurkerTrash(this, player.PlayerState, player.Game.Kingdom))
		{
			var cardSelection = player.Game.Kingdom.Where(c => !c.Empty && c.CardInstance.IsAction).Select(c => c.CardInstance).ToList();
			if (cardSelection.Count != 0)
			{
				CardInstance cardToTrash = player.User.LurkerChooseCardToTrash(this, player.PlayerState, player.Game.Kingdom,
					player.Game.Kingdom.Where(c => !c.Empty && c.CardInstance.IsAction).Select(c => c.CardInstance).ToList());
				player.Trash(cardToTrash);
			}
		}
		else
		{
			var cardSelection = player.Game.Trash.Where(c => c.IsAction).ToList();
			if (cardSelection.Count != 0)
			{
				CardInstance cardToGain = player.User.LurkerChooseCardToGain(
					this, player.PlayerState, player.Game.Kingdom, player.Game.Trash.Where(c => c.IsAction).ToList());
				player.Gain(cardToGain);
			}
		}
	}
}
