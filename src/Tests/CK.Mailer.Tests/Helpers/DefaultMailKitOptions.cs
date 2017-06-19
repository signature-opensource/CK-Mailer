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
                    SendMails = false,
                    UsePickupDirectory = true,
                    PickupDirectoryPath = "./PickupDirectory"
                };
            }
        }
    }
}
