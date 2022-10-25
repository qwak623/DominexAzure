using MimeKit;

namespace Dominex.Services.Mailing;

public interface IMailingService
{
	void Send(MimeMessage mailMessage);
}
