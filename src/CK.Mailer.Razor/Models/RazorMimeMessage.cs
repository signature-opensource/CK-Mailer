using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using RazorLight;
using RazorLight.Extensions;

namespace CK.Mailer.Razor
{
    public class RazorMimeMessage : SimpleMimeMessage, IRazorMimeMessage
    {
        BodyBuilder _body;
        public RazorMimeMessage()
            : base()
        {
            _body = new BodyBuilder();
        }
        
        public RazorMimeMessage( string to )
            : this()
        {
            To.Add( new MailboxAddress( to ) );
        }

        public RazorMimeMessage( string from, string to )
            : this( to )
        {
            From.Add( new MailboxAddress( from ) );
        }

        public RazorMimeMessage( string from, string to, string subject )
            : this( from, to )
        {
            Subject = subject;
        }

        public void SetRazorBody<T>( IRazorLightEngine engine, T model, string template )
        {
            SetRazorBody( engine, e => e.Parse( template, model ) );
        }

        public void SetRazorBody( IRazorLightEngine engine, Func<IRazorLightEngine, string> execute )
        {
            string razorResult = execute( engine );

            SetHtmlBody( razorResult );
        }

        public void SetRazorBodyFromString<T>( IRazorLightEngine engine, T model, string content )
        {
            SetRazorBody( engine, e => e.ParseString( content, model ) );
        }
    }
}
