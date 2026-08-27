using GameCore.GameCore;

namespace GameCore.Cards.Base;

public class Moneylender : Card
{
	private static Moneylender moneylender;
	private Moneylender() : base(CardType.Action)
	{
		Name = CardName.Moneylender;
		DefaultPrice = 4;
		moneylender = this;
		Description = "You may trash a Copper from your hand for +$3.";
	}

	public static Moneylender Get() => moneylender ?? new Moneylender();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Any(c => c.Card.Name == CardName.Copper)
			&& player.User.MoneylenderTrash(this, player.PlayerState, player.Game.Kingdom))
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} trashes Copper and gains 3$" });
			player.Trash(player.PlayerState.Hand.First(c => c.Card.Name == CardName.Copper));
			player.PlayerState.Coins += 3;
		}
	}
}
