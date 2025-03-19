using System.Collections.Generic;

namespace CK.Mailer;

internal sealed class InternalDefaultEmailSender : EmailSenderFeature, IDefaultEmailSender
{
    internal InternalDefaultEmailSender( IEnumerable<IEmailSender> senders ) : base( senders )
    {
    }
}
