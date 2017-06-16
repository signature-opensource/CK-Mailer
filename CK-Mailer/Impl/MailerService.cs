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
        MailKitOptions  _options;
        
        public MailerService( MailKitOptions options )
        {
            _options = options;
        }

        public async Task SendAsync<T>( IActivityMonitor m, MimeMessage message )
        {
            await StaticMailerService.SendMailAsync( m, message, _options );
        }
    }
}
