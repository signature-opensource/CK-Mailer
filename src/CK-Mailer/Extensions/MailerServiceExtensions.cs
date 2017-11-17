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
        public static Task SendAsync( this IMailerService @this, IActivityMonitor m, SimpleMimeMessage mailModel )
        {
            return @this.SendAsync( m, mailModel );
        }

        public static void Send( this IMailerService @this, IActivityMonitor m, SimpleMimeMessage mailModel )
        {
            @this.Send( m, mailModel );
        }
    }
}
