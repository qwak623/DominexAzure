namespace GameCore.Cards.Intrique;

public class Pawn : Card
{
	private static Pawn pawn;
	private Pawn() : base(CardType.Action)
	{
		Name = CardName.Pawn;
		DefaultPrice = 2;
		pawn = this;
		Description = "Choose two: +1 Card; +1 Action; +1 Buy; +$1. The choices must be different.";
	}

	public static Pawn Get() => pawn ?? new Pawn();

	private static readonly List<PawnBenefit> allBenefits = Enum.GetValues<PawnBenefit>().ToList();

	private static readonly Dictionary<PawnBenefit, Action<IPlayer>> benefitEffects = new()
	{
		[PawnBenefit.Card] = p => p.Draw(1),
		[PawnBenefit.Action] = p => p.PlayerState.Actions++,
		[PawnBenefit.Buy] = p => p.PlayerState.Buys++,
		[PawnBenefit.Coin] = p => p.PlayerState.Coins++,
	};

	protected override void ActionEffect(IPlayer player, CardInstance thisCard)
	{
		List<PawnBenefit> chosenBenefits = player.User.PawnChooseBenefits(this, player.PlayerState, player.Game.Kingdom, 2, allBenefits);
		chosenBenefits.ForEach(benefit => benefitEffects[benefit](player));
	}
}

public enum PawnBenefit
{
	Card,
	Action,
	Buy,
	Coin
}
