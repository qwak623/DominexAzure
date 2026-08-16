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



	public List<OperationType> ToOperationTypes(List<CourtierBenefit> benefits) => benefits.Select(b => courtierBenefitToOperationType[b]).ToList();
	public List<CourtierBenefit> ToCourtierBenefits(List<OperationType> operationTypes) => operationTypes.Select(o => operationTypeToCourtierBenefit[o]).ToList();
}
