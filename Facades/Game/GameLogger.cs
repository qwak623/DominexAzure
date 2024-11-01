using Dominex.Facades.Infrastructure;
using GameCore;
using Havit.Extensions.DependencyInjection.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Dominex.Facades.Game;

[Service]
internal class GameLogger : IGameLogger
{
	//private readonly string _fileName = "Log.txt";
	//private readonly StreamWriter _writer;

	private readonly IHubContext<LogHub> logHubContext;

	public GameLogger(IHubContext<LogHub> logHubContext)
	{
		this.logHubContext = logHubContext;
		//_writer = new StreamWriter(_fileName);
	}

	public async Task Log(string log)
	{
		await logHubContext.Clients.All.SendAsync("AppendLog", log);

		//_writer.WriteLine(str);
		//_writer.Flush(); // todo - přes destructor?
	}
}
