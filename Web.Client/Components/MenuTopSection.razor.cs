using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class MenuTopSection
{
	[Parameter] public Func<Task> ClickStart { get; set; }
}
