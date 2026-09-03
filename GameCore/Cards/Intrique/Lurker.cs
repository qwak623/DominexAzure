namespace GameCore.Cards.Intrique;
public class Lurker : Card
{
	private static Lurker lurker;
	private Lurker() : base(CardType.Action)
	{
		Name = CardName.Lurker;
		DefaultPrice = 2;
		AddActions = 1;
		lurker = this;
		Description = "Choose one: Trash an Action card from the Supply; or gain an Action card from the trash.";
	}

	public static Lurker Get() => lurker ?? new Lurker();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.User.LurkerTrash(this, player.PlayerState, player.Game.Kingdom))
		{
			var cardSelection = player.Game.Kingdom.Where(c => !c.Empty && c.CardInstance.IsAction).Select(c => c.CardInstance).ToList();
			CardInstance cardToTrash = player.User.LurkerChooseCardToTrash(this, player.PlayerState, player.Game.Kingdom,
				player.Game.Kingdom.Where(c => !c.Empty && c.CardInstance.IsAction).Select(c => c.CardInstance).ToList());
			player.Trash(cardToTrash);
		}
		else
		{
			var cardSelection = player.Game.Trash.Where(c => c.IsAction).ToList();
			CardInstance cardToGain = player.User.LurkerChooseCardToGain(
				this, player.PlayerState, player.Game.Kingdom, player.Game.Trash.Where(c => c.IsAction).ToList());
			player.Gain(cardToGain);
		}
	}
}
