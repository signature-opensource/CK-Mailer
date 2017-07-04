using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public interface IMailModel
    {
        RecipientModel Recipients { get; }

        string Subject { get; }
        string Body { get; }
        string TextBody { get; }

        AttachmentCollection Attachments { get; }

        MimeMessage ToMimeMessage();
    }
}
