using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Components;

public partial class SelectCardPanel
{
	[Parameter] public string Message { get; set; }
	[Parameter] public List<string> Cards { get; set; }
	[Parameter] public int SelectionCount { get; set; }
}
