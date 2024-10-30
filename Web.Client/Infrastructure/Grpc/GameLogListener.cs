using Havit.Blazor.Grpc.Client.ServerExceptions;
using Dominex.Resources;
using Microsoft.Extensions.Localization;
using Dominex.Contracts.Game.ClientApi;

namespace Dominex.Web.Client.Infrastructure.Grpc;

public class GameLogListener : IGameLogFacade
{
	private readonly IHxMessengerService messenger;
	private readonly IStringLocalizer<Global> localizer;

	public GameLogListener()
	{
	}

	public void Log(string message)
	{
	}
}
