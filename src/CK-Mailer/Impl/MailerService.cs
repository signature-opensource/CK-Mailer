using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using CK.Core;
using MailKit.Net.Smtp;

namespace CK.Mailer
{
    public class MailerService : IMailerService
    {
        public IMailKitClientProvider Provider { get; set; };
        
        public MailerService( IMailKitClientProvider provider )
        {
            Provider = provider;
        }

        public async Task SendAsync( IActivityMonitor m, MimeMessage message )
        {
            await StaticMailerService.SendMailAsync( m, message, Provider );
        }
    }
}
