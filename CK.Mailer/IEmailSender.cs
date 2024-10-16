using CK.Core;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

public interface IEmailSender
{
    Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default );
}
