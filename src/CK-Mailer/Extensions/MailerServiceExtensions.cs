using CK.Core;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class MailerServiceExtensions
    {
        public static Task SendAsync( this IMailerService @this, IActivityMonitor m, MailboxAddress from, MailboxAddress to, string subject, MimeEntity body )
        {
            var options = @this.Provider.Options;

            MimeMessage message = new MimeMessage();

            if( from != null ) message.From.Add( from );
            if( to != null ) message.From.Add( to );

            message.Subject = subject;

            message.Body = body;

            @this.SendAsync( m, message );
        }
    }
}
