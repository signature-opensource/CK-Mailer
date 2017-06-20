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
        public IMailKitClientProvider Provider { get; set; }
        
        public MailerService( IMailKitClientProvider provider )
        {
            Provider = provider;
        }

        public Task SendAsync( IActivityMonitor m, MimeMessage message )
        {
            return StaticMailerService.SendMailAsync( m, message, Provider );
        }

        public void Send( IActivityMonitor m, MimeMessage message )
        {
            StaticMailerService.SendMail( m, message, Provider );
        }
    }
}
