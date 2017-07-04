using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class BasicMailModel : IMailModel
    {
        public RecipientModel Recipients { get; set; }

        public string Subject { get; set; }
        public string Body
        {
            get
            {
                return _body.HtmlBody;
            }
            set
            {
                if( value != _body.HtmlBody )
                {
                    processBody( value );
                }
            }
        }
        public string TextBody { get { return _body.TextBody; } }

        public AttachmentCollection Attachments { get { return _body.Attachments; } }


        BodyBuilder _body;
        public BasicMailModel()
        {
            Recipients = new RecipientModel();
            _body = new BodyBuilder();
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
            : this( to )
        {
            Recipients.From.Add( new MailboxAddress( from ) );
        }

        public BasicMailModel( string to, string subject, string body )
            : this( to )
        {
            Subject = subject;
            processBody( body );
        }

        public BasicMailModel( string from, string to, string subject, string body )
            : this( from, to )
        {
            Subject = subject;
            processBody( body );
        }

        private void processBody( string body )
        {
            _body.HtmlBody = body;
            _body.TextBody = WebUtil.HtmlToText( body, true );
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
            message.Body = _body.ToMessageBody();

            return message;
        }
    }
}
