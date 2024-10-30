using Havit.Blazor.Grpc.Client.ServerExceptions;
using Dominex.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Runtime.Serialization;

namespace Dominex.Web.Client.Infrastructure.Grpc;

public class HxMessengerOperationFailedExceptionGrpcClientListener : IOperationFailedExceptionGrpcClientListener
{
	private readonly IHxMessengerService messenger;
	private readonly IStringLocalizer<Global> localizer;

	public HxMessengerOperationFailedExceptionGrpcClientListener(IHxMessengerService messenger, IStringLocalizer<Global> localizer)
	{
		this.messenger = messenger;
		this.localizer = localizer;
	}

	public Task ProcessAsync(string errorMessage)
	{
		// todo
		messenger.AddError(localizer["OperationFailedExceptionMessengerTitle"], errorMessage);


		//messenger.AddError(localizer["OperationFailedExceptionMessengerTitle"], "som čarovny");

		return Task.CompletedTask;
	}
}
