using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using CK.Core;
using MailKit.Net.Smtp;
using CK.Mailer.Razor;

namespace CK.Mailer
{
    public class LegacyRazorMailerService : MailerService, ILegacyRazorMailerService
    {
        public LegacyRazorMailerService( IMailKitClientProvider provider, IPhysicalPathProvider pathProvider )
            : base( provider )
        {
            PathProvider = pathProvider;
        }

        public IPhysicalPathProvider PathProvider { get; set; }
    }
}
