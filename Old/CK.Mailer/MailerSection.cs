using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public class MailerSection : ConfigurationSection
    {
        [ConfigurationProperty( "isRecipientWhiteListed", DefaultValue = "false", IsRequired = false )]
        public bool IsRecipientWhiteListed
        {
            get
            {
                return (bool)this["isRecipientWhiteListed"];
            }
            set
            {
                this["isRecipientWhiteListed"] = value;
            }
        }


        [ConfigurationProperty( "whiteList", IsRequired = false, IsDefaultCollection = true )]
        public WhiteListCollection WhiteList
        {
            get
            {
                return (WhiteListCollection)this["whiteList"];
            }
            set
            {
                this["whiteList"] = value;
            }
        }
    }

    public class WhiteListCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WhiteListRecipient();
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ((WhiteListRecipient)element).Email;
        }
    }
    
    public class WhiteListRecipient : ConfigurationElement
    {
        [ConfigurationProperty( "email", IsKey = true, IsRequired = true )]
        public string Email
        {
            get { return (string)base["email"]; }
            set { base["email"] = value; }
        }
    }
}