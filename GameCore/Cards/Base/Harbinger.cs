using GameCore.GameCore;

namespace GameCore.Cards.Base;
public class Harbinger : Card
{
	private static Harbinger harbinger;
	private Harbinger() : base(CardType.Action)
	{
		Name = CardName.Harbinger;
		DefaultPrice = 3;
		AddActions = 1;
		DrawCards = 1;
		harbinger = this;
		Description = "Look through your discard pile. You may put a card from it onto your deck.";
	}

	public static Harbinger Get() => harbinger ?? new Harbinger();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var card = player.User.HarbingerPutOnTop(
			this, player.PlayerState, player.Game.Kingdom, player.PlayerState.DiscardPile.ToList());
		player.ReturnToDrawPile(card);
	}
}
