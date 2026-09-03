using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Feast : Card
{
	private static Feast feast;
	private Feast() : base(CardType.Action)
	{
		Name = CardName.Feast;
		DefaultPrice = 4;
		feast = this;
		Description = "Trash this card. Gain a card costing up to $5.";
	}

	public static Feast Get() => feast ?? new Feast();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		player.Trash(thisCard);
		player.Game.Logger?.Log(new GameLog { PlayerId = player.Name, Message = $"{player.Name} trashes {Name.ToDisplayName()}." });
		var availableCards = player.Game.Kingdom.GetWrapper(player.PlayerState, 5).AvailableCards.ToList();
		var card = player.User.SelectCardToGain(this, player.PlayerState, player.Game.Kingdom, availableCards);
		player.Gain(card);
	}
}
