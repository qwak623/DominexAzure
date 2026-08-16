using Dominex.Contracts.Game;
using GameCore.Cards.Intrique;

namespace Dominex.Services.Game;
public interface IOperationMapper
{
	List<OperationType> ToOperationTypes(List<CourtierBenefit> benefits);
	List<CourtierBenefit> ToCourtierBenefits(List<OperationType> operationTypes);
}
