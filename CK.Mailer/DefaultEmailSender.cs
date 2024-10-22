using System.Collections.Generic;

namespace CK.Mailer;

public class DefaultEmailSender : EmailSenderFeature, IDefaultEmailSender
{
    public DefaultEmailSender( IEnumerable<IEmailSender> senders ) : base( senders )
    {
    }
}
