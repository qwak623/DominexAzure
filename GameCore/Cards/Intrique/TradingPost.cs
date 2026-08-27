namespace GameCore.Cards.Intrique;
public class TradingPost : Card
{
	private static TradingPost tradingPost = null;
	private TradingPost() : base(CardType.Action)
	{
		Name = CardName.TradingPost;
		DefaultPrice = 5;
		tradingPost = this;
		Description = "Trash 2 cards from your hand. If you did, gain a Silver to your hand.";
	}

	public static TradingPost Get() => tradingPost ?? new TradingPost();

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		List<CardInstance> cardsToTrash = player.PlayerState.Hand.Count <= 2
			? player.PlayerState.Hand.ToList()
			: player.User.TradingPostTrash(this, player.PlayerState, player.Game.Kingdom, player.PlayerState.Hand.ToList());
		cardsToTrash.ForEach(player.Trash);
		if (cardsToTrash.Count == 2)
		{
			player.GainToHand(CardName.Silver);
		}
	}
}

