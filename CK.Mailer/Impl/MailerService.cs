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

        
        public Task SendAsync( IActivityMonitor m, MimeMessage message, ITransferProgress progress = null, CancellationToken cancellationToken = default )
        {
            return StaticMailerService.SendMailAsync( m, Provider, message, progress, cancellationToken );
        }

        public Task SendAsync( IActivityMonitor m, FormatOptions formatOptions, MimeMessage message, ITransferProgress progress = null, CancellationToken cancellationToken = default )
        {
            return StaticMailerService.SendMailAsync( m, Provider, formatOptions, message, progress, cancellationToken );
        }
    }
}
