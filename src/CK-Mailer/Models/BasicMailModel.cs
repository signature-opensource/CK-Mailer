using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class BasicMailModel
    {
        public RecipientModel Recipients { get; set; }

        public string Subject { get; set; }
        public BodyBuilder Body { get; set; }

        public AttachmentCollection Attachments { get; private set; }
        
        public BasicMailModel()
        {
            Recipients = new RecipientModel();
            Body = new BodyBuilder();
            Attachments = Body.Attachments;
        }

        /// <summary>
        /// This contructor allow the IMailerService to use the MailKitOptions.DefaultSenderEmail
        /// </summary>
        /// <param name="to"></param>
        public BasicMailModel( string to )
            : this()
        {
            Recipients.To.Add( new MailboxAddress( to ) );
        }

        public BasicMailModel( string from, string to )
            : this(to)
        {
            Recipients.From.Add( new MailboxAddress( from ) );
        }

        public BasicMailModel( string to, string subject, string textBody )
            : this( to )
        {
            Subject = subject;
            Body.TextBody = textBody;
        }

        public BasicMailModel(string from, string to, string subject, string textBody)
            : this(from, to)
        {
            Subject = subject;
            Body.TextBody = textBody;
        }

        public MimeMessage ToMimeMessage()
        {
            MimeMessage message = new MimeMessage();

            message.Bcc.AddRange( Recipients.Bcc );
            message.Cc.AddRange( Recipients.Cc );
            message.From.AddRange( Recipients.From );
            message.ReplyTo.AddRange( Recipients.ReplyTo );
            message.ResentBcc.AddRange( Recipients.ResentBcc );
            message.ResentCc.AddRange( Recipients.ResentCc );
            message.ResentFrom.AddRange( Recipients.ResentFrom );
            message.ResentReplyTo.AddRange( Recipients.ResentReplyTo );
            message.ResentTo.AddRange( Recipients.ResentTo );
            message.To.AddRange( Recipients.To );

            message.Subject = Subject;
            message.Body = Body.ToMessageBody();

            return message;
        }
    }
}
