using Microsoft.AspNetCore.SignalR;

namespace Dominex.Facades.Infrastructure;

public class LogHub : Hub
{
	//private static readonly string _fileName = "Log.txt";
	//private static readonly StreamWriter _writer = new StreamWriter(_fileName);
	//private readonly IHubContext<LogHub> logHubContext;

	//public async Task Log(string log)
	//{
	//	//_writer.WriteLine(log);
	//	//_writer.Flush();

	//	await Clients.All.SendAsync("AppendLog", log);
	//}
}
