using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class MenuTopSection
{
	[Parameter] public Func<Task> ClickStart { get; set; }
	[Parameter] public Action ClickLoadSettings { get; set; }
	[Parameter] public Action ClickSaveSettings { get; set; }
	[Parameter] public Action ClickLoadGame { get; set; }

}
