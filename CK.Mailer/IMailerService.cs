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

        Task SendAsync( IActivityMonitor m, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null );
        Task SendAsync( IActivityMonitor m, FormatOptions formatOptions, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null );
        
        void Send( IActivityMonitor m, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null );
        void Send( IActivityMonitor m, FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null );
    }
}
