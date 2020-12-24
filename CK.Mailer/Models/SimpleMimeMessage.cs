using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CK.Mailer
{
    public class SimpleMimeMessage : MimeMessage
    {
        public bool BodyIsProcessed { get; protected set; }

        protected BodyBuilder BodyBuilder;
        public SimpleMimeMessage()
            : base()
        {
            BodyBuilder = new BodyBuilder();
        }

        /// <summary>
        /// This contructor allow the IMailerService to use the MailKitOptions.DefaultSenderEmail
        /// </summary>
        /// <param name="to"></param>
        public SimpleMimeMessage( string to )
            : this()
        {
            To.Add( MailboxAddress.Parse( to ) );
        }

        public SimpleMimeMessage( string from, string to )
            : this( to )
        {
            From.Add( MailboxAddress.Parse( from ) );
        }

        public SimpleMimeMessage( string to, string subject, string body )
            : this( to )
        {
            Subject = subject;
            ProcessBody( body );
        }

        public SimpleMimeMessage( string from, string to, string subject, string body )
            : this( from, to )
        {
            Subject = subject;
            ProcessBody( body );
        }

        private void ProcessBody( string body )
        {
            BodyBuilder.HtmlBody = body;
            BodyBuilder.TextBody = WebUtil.HtmlToText( body, true );

            Body = BodyBuilder.ToMessageBody();

            BodyIsProcessed = true;
        }

        public void SetHtmlBody( string body )
        {
            ProcessBody( body );
        }

        public MimeEntity AddAttachment( string fileName, Stream stream, ContentType contentType )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName, stream, contentType );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
        public MimeEntity AddAttachment( string fileName, byte[] data )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName, data );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
        public MimeEntity AddAttachment( string fileName, Stream stream )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName, stream );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
        public MimeEntity AddAttachment( string fileName, ContentType contentType )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName, contentType );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
        public MimeEntity AddAttachment( string fileName )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
        public MimeEntity AddAttachment( MimeEntity attachment )
        {
            BodyBuilder.Attachments.Add( attachment );

            Body = BodyBuilder.ToMessageBody();

            return attachment;
        }
        public MimeEntity AddAttachment( string fileName, byte[] data, ContentType contentType )
        {
            var mimeEntity = BodyBuilder.Attachments.Add( fileName, data, contentType );

            Body = BodyBuilder.ToMessageBody();

            return mimeEntity;
        }
    }
}
