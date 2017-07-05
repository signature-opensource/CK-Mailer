using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class Recipient
    {
        public Recipient()
        {
        }

        public Recipient( string emailAddress, string displayName = null )
        {
            EmailAddress = emailAddress;
            DisplayName = displayName;
        }

        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
    }
}
