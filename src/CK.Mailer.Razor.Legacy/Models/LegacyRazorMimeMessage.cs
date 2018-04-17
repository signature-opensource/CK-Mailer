using MimeKit;
using RazorEngine;
using RazorEngine.Templating; // For extension methods.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CK.Mailer.Razor
{
    public class LegacyRazorMimeMessage : SimpleMimeMessage, ILegacyRazorMimeMessage
    {
        BodyBuilder _body;
        public LegacyRazorMimeMessage()
            : base()
        {
            _body = new BodyBuilder();
        }
        
        public LegacyRazorMimeMessage( string to )
            : this()
        {
            To.Add( new MailboxAddress( to ) );
        }

        public LegacyRazorMimeMessage( string from, string to )
            : this( to )
        {
            From.Add( new MailboxAddress( from ) );
        }

        public LegacyRazorMimeMessage( string from, string to, string subject )
            : this( from, to )
        {
            Subject = subject;
        }

        public void SetRazorBody<T>( IPhysicalPathProvider provider, T model, string templateVirtualPath )
        {
            string templatePath = provider.MapPath( templateVirtualPath );
            if( !File.Exists( templatePath ) ) throw new FileNotFoundException( $"Template not found from {templatePath}" );

            string content = System.IO.File.ReadAllText( templatePath );
            SetRazorBodyFromString( model, content );
        }

        public void SetRazorBodyFromString<T>( T model, string content )
        {
            var result = Engine.Razor.RunCompile( content, content.GetHashCode().ToString(), null, model );
            SetHtmlBody( result );
        }
    }
}
