using CK.Core;
using CK.Mailer.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

public interface IEmailSender : IAutoService
{
    SimpleEmail Configure( Action<SimpleEmail> configure );

    Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default );
}
