using Dominex.Contracts.Game;

namespace Dominex.Contracts.ServerApi;

[ApiContract]
public interface IGameFacade
{
	Task Start(CancellationToken cancellationToken = default);
	Task StartWithCards(IEnumerable<string> cardTypes, CancellationToken cancellationToken = default);
	Task<ChoiceDto> JoinGame(/*Dto<Guid> gameId,*/ Dto<int> playerId, CancellationToken cancellationToken = default);
	Task<ChoiceDto> Submit(Answer answer, CancellationToken cancellationToken = default);
	Task RequestKingdomNotification(CancellationToken cancellationToken = default);
	Task RequestPlayerStateNotification(CancellationToken cancellationToken = default);
}
