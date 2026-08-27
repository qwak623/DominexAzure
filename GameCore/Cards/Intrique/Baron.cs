using GameCore.GameCore;

namespace GameCore.Cards.Intrique;

public class Baron : Card
{
	private static Baron baron;
	private Baron() : base(CardType.Action)
	{
		Name = CardName.Baron;
		DefaultPrice = 4;
		AddBuys = 1;
		baron = this;
		Description = "You may discard an Estate for +$4. If you don't, gain an Estate.";
	}

	public static Baron Get() => baron ?? new Baron();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Any(c => c.Card.Name == CardName.Estate)
			&& player.User.BaronDiscard(this, player.PlayerState, player.Game.Kingdom))
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} discards Estate and gains 4$" });
			player.Discard(player.PlayerState.Hand.First(c => c.Card.Name == CardName.Estate));
			player.PlayerState.Coins += 4;
		}
		else
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} gains Estate" });
			player.Gain(CardName.Estate);
		}
	}
}
