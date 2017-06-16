using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class MailKitOptions
    {
        public MailKitOptions()
        {

        }

        /// <summary>
        /// SMTP Host address
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP Host  Port ,default is 25
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// Default sender name used in "From" field associated with his e-mail
        /// </summary>
        public string DefaultSenderName { get; set; }

        /// <summary>
        /// Default sender e-mail
        /// </summary>
        public string DefaultSenderEmail { get; set; }

        /// <summary>
        /// User used to be login from the host
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password used to be login from the host
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// enable ssl 
        /// </summary>
        public bool UseSSL { get; set; } = false;


        public string PreferredEncoding { get; set; } = String.Empty;
        public int ShootingDelay { get; set; }

        public bool SendMails { get; set; }
        public bool? UsePickupDirectory { get; set; }
        public string PickupDirectoryPath { get; set; }
    }
}
