namespace GameCore.Cards.Intrique;
public class Ironworks : Card
{
	private static Ironworks ironworks;
	private Ironworks() : base
	(
		type: CardName.Ironworks,
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
		ironworks = this;
		Description = $"Gain a card costing up to $4. If the gained card is an...{Environment.NewLine}Action card, +1 Action{Environment.NewLine}Treasure card, +$1{Environment.NewLine}Victory card, +1 Card";
	}

	public static Ironworks Get() => ironworks ?? new Ironworks();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var card = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(p.PlayerState, 4), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (card is null)
		{
			return;
		}
		p.Gain(card);
		if (card.Card.IsAction)
		{
			p.PlayerState.Actions++;
		}
		if (card.Card.IsTreasure)
		{
			p.PlayerState.Coins++;
		}
		if (card.Card.IsVictory)
		{
			p.Draw(1);
		}
	}
}
