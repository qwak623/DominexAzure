using Dominex.Resources;
using Microsoft.Extensions.Localization;
using Dominex.Contracts.Game.ClientApi;

namespace Dominex.Web.Client.Infrastructure.Grpc;

// todo co je tohle? nezůstalo to tady?
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
