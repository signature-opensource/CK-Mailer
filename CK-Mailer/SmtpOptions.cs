using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class SmtpOptions
    {
        public string Host { get; set; } = String.Empty;
        public int Port { get; set; } = 25;
        public string User { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public bool UseSsl { get; set; } = false;
        public string PreferredEncoding { get; set; } = String.Empty;
        public int ShootingDelay { get; set; }

        public string From { get; set; }

        public bool SendMails { get; set; }
        public bool? UsePickupDirectory { get; set; }
        public string PickupDirectory { get; set; }
    }
}
