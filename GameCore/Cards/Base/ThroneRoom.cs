namespace GameCore.Cards.Base;
public class ThroneRoom : Card
{
	private static ThroneRoom throneRoom;
	private ThroneRoom() : base(CardType.Action)
	{
		Name = CardName.ThroneRoom;
		DefaultPrice = 4;
		throneRoom = this;
		Description = "You may play an Action card from your hand twice.";
		Message = "You may play an Action card from your hand twice.";
	}

	public static ThroneRoom Get() => throneRoom ?? new ThroneRoom();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		var actionCards = player.PlayerState.Hand.Where(c => c.IsAction);
		var cardInstance = player.User.ThroneRoomPlay(this, player.PlayerState, player.Game.Kingdom, actionCards.ToList());
		if (cardInstance is null)
		{
			return;
		}

		player.PlayerState.CardsPlayed.Move(cardInstance);
		var card = cardInstance.Card;
		for (int i = 0; i < 2; i++)
		{
			player.PlayerState.ActionsPlayed.Add(card);
			cardInstance.WhenPlayAction(player);
		}
	}
}
