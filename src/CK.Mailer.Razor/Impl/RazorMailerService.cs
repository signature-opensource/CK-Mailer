using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using CK.Core;
using MailKit.Net.Smtp;
using RazorLight;
using CK.Mailer.Razor;

namespace CK.Mailer
{
    public class RazorMailerService : IRazorMailerService
    {
        public IMailKitClientProvider Provider { get; set; }
        public IRazorLightEngine RazorEngine { get; set; }

        public RazorMailerService( IMailKitClientProvider provider, IRazorLightEngine razorEngine )
        {
            Provider = provider;
            RazorEngine = razorEngine;
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
