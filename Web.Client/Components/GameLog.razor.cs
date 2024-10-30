using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace Dominex.Web.Client.Components;

public partial class GameLog
{
	[Parameter] public List<string> Log { get; set; } = new List<string>();

	public class LogHub : Hub
	{
		public async Task SendMessage(string user, string message)
		{
			await Clients.All.SendAsync("ReceiveMessage", user, message);
		}
	}
}
