using Dominex.Contracts.Game;
using Havit.ComponentModel;

namespace Dominex.Contracts.ServerApi;

[ApiContract]
public interface IGameFacade
{
	Task Start(CancellationToken cancellationToken = default);
	Task<Choice> JoinGame(/*Dto<Guid> gameId,*/ Dto<int> playerId, CancellationToken cancellationToken = default);
	Task<Choice> SelectCard(string card, CancellationToken cancellationToken = default);
	Task RequestKingdomNotification(CancellationToken cancellationToken = default);
	Task RequestPlayerStateNotification(CancellationToken cancellationToken = default);
}
