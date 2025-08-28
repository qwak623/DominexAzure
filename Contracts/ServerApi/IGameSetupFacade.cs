using Dominex.Contracts.Game;

namespace Dominex.Contracts.ServerApi;

[ApiContract]
public interface IGameSetupFacade
{
	Task<List<CardDto>> RequestAvailableCards();
}
