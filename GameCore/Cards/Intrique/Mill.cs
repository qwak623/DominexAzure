using GameCore.GameCore;

namespace GameCore.Cards.Intrique;

public class Mill : Card
{
	private static Mill mill;
	private Mill() : base([CardType.Action, CardType.Victory])
	{
		Name = CardName.Mill;
		DefaultPrice = 4;
		AddActions = 1;
		DrawCards = 1;
		mill = this;
		VictoryPoints = 1;
		Description = "You may discard 2 cards. If you do, +$2";
	}

	public static Mill Get() => mill ?? new Mill();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		if (player.PlayerState.Hand.Count == 0 || !player.User.MillWantsToDiscard(this, player.PlayerState, player.Game.Kingdom))
		{
			return;
		}

		List<CardInstance> cardsToDiscard = player.User.MillChooseCardsToDiscard(this, player.PlayerState, player.Game.Kingdom,
			player.PlayerState.Hand.ToList(), Math.Min(2, player.PlayerState.Hand.Count));
		cardsToDiscard.ForEach(player.Discard);
		if (cardsToDiscard.Count == 2)
		{
			player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} discards two cards and gains $2" });
			player.PlayerState.Coins += 2;
		}
	}
}
