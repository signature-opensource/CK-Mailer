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
    public class RazorMailerService : MailerService, IRazorMailerService
    {
        public IRazorLightEngine RazorEngine { get; set; }

        public RazorMailerService( IMailKitClientProvider provider, IRazorLightEngine razorEngine )
            : base( provider )
        {
            RazorEngine = razorEngine;
        }
    }
}
