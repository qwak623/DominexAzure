using Dominex.Contracts.Game;
using GameCore.Cards.Intrique;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace Dominex.Services.Game;

[Service]
public class OperationMapper : IOperationMapper
{
	private static readonly Dictionary<CourtierBenefit, OperationType> courtierBenefitToOperationType = new()
	{
		[CourtierBenefit.Action] = OperationType.AddActions,
		[CourtierBenefit.Buy] = OperationType.AddBuys,
		[CourtierBenefit.Coins] = OperationType.AddCoins,
		[CourtierBenefit.GainGold] = OperationType.Gain,
	};

	private static readonly Dictionary<OperationType, CourtierBenefit> operationTypeToCourtierBenefit
		= courtierBenefitToOperationType.ToDictionary(kv => kv.Value, kv => kv.Key);

	private static readonly Dictionary<PawnBenefit, OperationType> pawnBenefitToOperationType = new()
	{
		[PawnBenefit.Card] = OperationType.Draw,
		[PawnBenefit.Action] = OperationType.AddActions,
		[PawnBenefit.Buy] = OperationType.AddBuys,
		[PawnBenefit.Coin] = OperationType.AddCoins,
	};

	private static readonly Dictionary<OperationType, PawnBenefit> operationTypeToPawnBenefit
		= pawnBenefitToOperationType.ToDictionary(kv => kv.Value, kv => kv.Key);

	private static readonly Dictionary<StewardBenefit, OperationType> stewardBenefitToOperationType = new()
	{
		[StewardBenefit.Cards] = OperationType.Draw,
		[StewardBenefit.Coins] = OperationType.AddCoins,
		[StewardBenefit.Trash] = OperationType.Trash,
	};

	private static readonly Dictionary<OperationType, StewardBenefit> operationTypeToStewardBenefit
		= stewardBenefitToOperationType.ToDictionary(kv => kv.Value, kv => kv.Key);


	public List<OperationType> ToOperationTypes(List<CourtierBenefit> benefits) => benefits.Select(b => courtierBenefitToOperationType[b]).ToList();
	public List<CourtierBenefit> ToCourtierBenefits(List<OperationType> operationTypes) => operationTypes.Select(o => operationTypeToCourtierBenefit[o]).ToList();

	public List<OperationType> ToOperationTypes(List<PawnBenefit> benefits) => benefits.Select(b => pawnBenefitToOperationType[b]).ToList();
	public List<PawnBenefit> ToPawnBenefits(List<OperationType> operationTypes) => operationTypes.Select(o => operationTypeToPawnBenefit[o]).ToList();

	public List<OperationType> ToOperationTypes(List<StewardBenefit> benefits) => benefits.Select(b => stewardBenefitToOperationType[b]).ToList();
	public List<StewardBenefit> ToStewardBenefits(List<OperationType> operationTypes) => operationTypes.Select(o => operationTypeToStewardBenefit[o]).ToList();
}
