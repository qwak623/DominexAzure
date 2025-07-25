using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Feast : Card
{
	private static Feast feast;
	private Feast() : base
	(
		name: "Feast",
		type: CardType.Feast,
		price: 4,
		addActions: 0,
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
		feast = this;
		Description = "Trash this card. Gain a card costing up to $5.";
	}

	public static Feast Get() => feast ?? new Feast();

	protected override void ActionEffect(IPlayer player)
	{
		player.PlayerState.PlayedCards.Remove(this);
		player.Game.Trash.Add(this);
		player.Game.Logger?.Log(new GameLog { PlayerId = Name, Message = $"{player.Name} trashes {Name}" });
		var card = player.User.SelectCardToGain(player.Game.Kingdom.GetWrapper(5), player.PlayerState, player.Game.Kingdom, Phase.Gain);
		if (card != null)
		{
			player.Gain(card.Type);
		}
	}
}
