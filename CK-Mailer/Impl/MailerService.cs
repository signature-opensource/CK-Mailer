using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using CK.Core;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

namespace CK.Mailer
{
    public class MailerService : IMailerService
    {
        SmtpOptions _smtpOptions;
        public MailerService( IOptions<SmtpOptions> smtpOptions )
            : this( smtpOptions.Value )
        {
        }

        public MailerService( SmtpOptions smtpOptions )
        {
            _smtpOptions = smtpOptions;
        }

        public async Task SendAsync<T>( IActivityMonitor m, MimeMessage message )
        {
            await StaticMailerService.SendMailAsync( m, message, _smtpOptions );
        }
    }
}
