using CK.Core;
using MailKit;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailerService
    {
        IMailKitClientProvider Provider { get; set; }

        Task SendAsync( IActivityMonitor m, MimeMessage message, ITransferProgress progress = null, CancellationToken cancellationToken = default );
        Task SendAsync( IActivityMonitor m, FormatOptions formatOptions, MimeMessage message, ITransferProgress progress = null, CancellationToken cancellationToken = default );
    }
}
