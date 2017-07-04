using CK.Mailer.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer.Tests
{
    public static class DefaultMailKitOptions
    {
        public static MailKitOptions Default
        {
            get
            {
                return new MailKitOptions()
                {
                    DefaultSenderEmail = "default@ckmailer.org",
                    DefaultSenderName = "ckmailer",
                    SendMails = false,
                    UsePickupDirectory = true,
                    PickupDirectoryPath = "./PickupDirectory"
                };
            }
        }


        public static RazorMailKitOptions DefaultRazor
        {
            get
            {
                return new RazorMailKitOptions()
                {
                    DefaultSenderEmail = "default@ckmailer.org",
                    DefaultSenderName = "ckmailer",
                    SendMails = false,
                    UsePickupDirectory = true,
                    PickupDirectoryPath = "./PickupDirectory",
                    ViewsPhysicalPath = "/"
                };
            }
        }
    }
}
