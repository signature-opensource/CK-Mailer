using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using RazorLight;
using RazorLight.Extensions;

namespace CK.Mailer.Razor
{
    public class RazorMailModel<T> : IRazorMailModel<T>
    {
        public Recipients Recipients { get; set; }

        public string Subject { get; set; }

        public T Model { get; set; }
        
        public AttachmentCollection Attachments { get { return _body.Attachments; } }

        BodyBuilder _body;
        public RazorMailModel()
        {
            Recipients = new Recipients();
            _body = new BodyBuilder();
        }

        public RazorMailModel( T model )
            : this()
        {
            Model = model;
        }

        public MimeMessage ProcessRazorView( IRazorLightEngine engine, string template )
        {
            return ProcessRazorView( engine, e => e.Parse( template, Model ) );
        }

        public MimeMessage ProcessRazorView( IRazorLightEngine engine, Func<IRazorLightEngine, string> execute )
        {
            _body.HtmlBody = execute( engine );

            _body.TextBody = WebUtil.HtmlToText( _body.HtmlBody, true );
            
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

        public MimeMessage ProcessRazorString( IRazorLightEngine engine, string content )
        {
            return ProcessRazorView( engine, e => e.ParseString( content, Model ) );
        }

        public MimeMessage ProcessRazorView( IRazorLightEngine engine )
        {
            throw new NotImplementedException();
        }
    }
}
