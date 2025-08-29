using Dominex.Contracts.Game;
using Dominex.Contracts.Menu;

namespace Dominex.Contracts.ServerApi;

[ApiContract]
public interface IGameSetupFacade
{
	Task<List<CardDto>> RequestAvailableCards();
	Task<List<PresetKingdomDto>> RequestPresetKingdoms();
	Task<GetRandomCardsResponse> GetRandomCards(GetRandomCardsRequest request);
}
