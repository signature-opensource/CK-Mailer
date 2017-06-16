using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    [Serializable]
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