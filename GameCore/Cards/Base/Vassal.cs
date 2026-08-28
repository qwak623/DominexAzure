namespace GameCore.Cards.Base;
public class Vassal : Card
{
	private static Vassal vassal;
	private Vassal() : base(CardType.Action)
	{
		Name = CardName.Vassal;
		DefaultPrice = 3;
		AddCoins = 2;
		vassal = this;
		Description = "Discard the top card of your deck. If it's an Action card, you may play it.";
	}

	public static Vassal Get() => vassal ?? new Vassal();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var card = player.Show(1).SingleOrDefault();
		if (card == null)
		{
			return;
		}

		if (card.IsAction && player.User.VassalPlay(this, player.PlayerState, player.Game.Kingdom, card))
		{
			player.PlayerState.CardsPlayed.Move(card);
			player.PlayerState.ActionsPlayed.Add(card.Card);
			card.WhenPlayAction(player);
		}
		else
		{
			player.Discard(card);
		}
	}
}
