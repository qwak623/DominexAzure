namespace GameCore.Cards.Base;
public class Workshop : Card
{
	private static Workshop workshop;
	private Workshop() : base(CardType.Action)
	{
		Name = CardName.Workshop;
		DefaultPrice = 3;
		workshop = this;
		Description = "Gain a card costing up to $4.";
	}

	public static Workshop Get() => workshop ?? new Workshop();

	protected override void ActionEffect(IPlayer p, CardInstance thisCard)
	{
		var card = p.User.SelectCardToGain(p.Game.Kingdom.GetWrapper(p.PlayerState, 4), p.PlayerState, p.Game.Kingdom, Phase.Gain);
		if (card is not null)
		{
			p.Gain(card);
		}
	}
}
