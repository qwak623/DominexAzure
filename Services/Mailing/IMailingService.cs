using MimeKit;

// todo odstranit mailing? mohlo by se teoreticky hodit
namespace Dominex.Services.Mailing;

public interface IMailingService
{
	Task VerifyHealthAsync(CancellationToken cancellationToken = default);

	Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default);
}
