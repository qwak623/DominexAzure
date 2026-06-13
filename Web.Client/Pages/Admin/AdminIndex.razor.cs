using Blazored.LocalStorage;
using Dominex.Contracts.Infrastructure;
using Dominex.Web.Client.Pages.Admin.Components;
using Dominex.Web.Client.Resources;
using Dominex.Web.Client.Resources.Pages.Admin;
using Microsoft.AspNetCore.Components;

namespace Dominex.Web.Client.Pages.Admin;

public partial class AdminIndex : ComponentBase
{
	[Inject] protected IMaintenanceFacade MaintenanceFacade { get; set; }
	[Inject] protected IHxMessengerService Messenger { get; set; }
	[Inject] protected IHxMessageBoxService MessageBox { get; set; }
	[Inject] protected ILocalStorageService LocalStorageService { get; set; }
	// TODO
	//[Inject] protected INavigationLocalizer NavigationLocalizer { get; set; }
	//[Inject] protected IAdminIndexLocalizer AdminIndexLocalizer { get; set; }
	[Inject] protected NavigationManager NavigationManager { get; set; }

	private DataSeeds _dataSeedsComponent;

	private async Task HandleRemoveCultureFromLocalStorageClick()
	{
		if (await MessageBox.ConfirmAsync("Do you really want to remove culture cache?"))
		{
			await LocalStorageService.RemoveItemAsync("culture");
			// TODO
			//Messenger.AddInformation(AdminIndexLocalizer["CultureRemoved"]); // TODO Just a demo
		}
	}

	private async Task HandleClearCacheClick()
	{
		if (await MessageBox.ConfirmAsync("Do you really want to clear server cache?"))
		{
			await MaintenanceFacade.ClearCacheAsync();

			if (await MessageBox.ConfirmAsync("Server cache cleared. Do you want to reload the Blazor client?"))
			{
				NavigationManager.NavigateTo("", forceLoad: true);
			}
		}
	}
}