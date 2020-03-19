using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using CK.Core;
using MailKit.Net.Smtp;
using System.Threading;
using MailKit;

namespace CK.Mailer
{
    public class MailerService : IMailerService
    {
        public IMailKitClientProvider Provider { get; set; }
        
        public MailerService( IMailKitClientProvider provider )
        {
            Provider = provider;
        }

        
        public Task SendAsync( IActivityMonitor m, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            return StaticMailerService.SendMailAsync( m, Provider, message, cancellationToken, progress );
        }

        public Task SendAsync( IActivityMonitor m, FormatOptions formatOptions, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            return StaticMailerService.SendMailAsync( m, Provider, formatOptions, message, cancellationToken, progress );
        }

        
        public void Send( IActivityMonitor m, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            StaticMailerService.SendMail( m, Provider, message, cancellationToken, progress );
        }

        public void Send( IActivityMonitor m, FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            StaticMailerService.SendMail( m, Provider, message, cancellationToken, progress );
        }

    }
}
