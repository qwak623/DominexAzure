using Dominex.Contracts.Game;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class InfoPanel
{
	[Parameter] public InfoDto InfoDto { get; set; }
}
