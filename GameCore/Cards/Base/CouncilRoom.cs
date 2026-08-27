namespace GameCore.Cards.Base;
public class CouncilRoom : Card
{
	private static CouncilRoom councilRoom;
	private CouncilRoom() : base(CardType.Action)
	{
		Name = CardName.CouncilRoom;
		DefaultPrice = 5;
		AddBuys = 1;
		DrawCards = 4;
		councilRoom = this;
		Description = "Each other player draws a card.";
	}

	public static CouncilRoom Get() => councilRoom ?? new CouncilRoom();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		foreach (var plr in player.Game.Players.Where(p => p != player))
		{
			plr.Draw(1);
		}
	}
}