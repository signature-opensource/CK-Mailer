using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public static class InternetAddressListExtensions
    {
        public static void Add( this InternetAddressList @this, string mailAddress )
        {
            @this.Add( MailboxAddress.Parse( mailAddress ) );
        }
    }
}
