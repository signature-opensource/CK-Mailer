using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using CK.Mailer.YodiiScript;
using Yodii.Script;
using CK.Core;
using System.Text.RegularExpressions;
using System.Net;

namespace CK.Mailer.YodiiScript
{
    public class YodiiScriptMimeMessage : SimpleMimeMessage, IYodiiScriptMimeMessage
    {
        readonly static Regex _commentTag = new Regex( "<!--.*?-->", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        BodyBuilder _body;
        public YodiiScriptMimeMessage()
            : base()
        {
            _body = new BodyBuilder();
        }
        
        public YodiiScriptMimeMessage( string to )
            : this()
        {
            To.Add( new MailboxAddress( to ) );
        }

        public YodiiScriptMimeMessage( string from, string to )
            : this( to )
        {
            From.Add( new MailboxAddress( from ) );
        }

        public YodiiScriptMimeMessage( string from, string to, string subject )
            : this( from, to )
        {
            Subject = subject;
        }

        public TemplateEngine.Result SetBodyFromYodiiScriptString<T>( IActivityMonitor m, T model, string content, bool escapeHtmlModelChars = true )
        {
            m.Trace().Send( "Create Yodii.Script.GlobalContext" );

            var c = new GlobalContext();

            m.Trace().Send( "Register Yodii.Script template Model" );

            c.Register( "$helper", new YodiiScriptHtmlHelpers() );
            c.Register( "Model", model );

            m.Trace().Send( "Create Yodii.Script.TemplateEngine" );

            var e = new TemplateEngine( c );

            e.SetWriteTransform( (s,sb) => sb.Append( _commentTag.Replace( s, String.Empty ) ) );
            
            if( escapeHtmlModelChars )
            {
                e.SetWriteTransform( ( s, sb ) => sb.Append( WebUtility.HtmlEncode( s ) ) );
            }

            m.Trace().Send( "Process Yodii.Script template" );

            content = _commentTag.Replace( content, String.Empty );

            var result = e.Process( content );

            if( !String.IsNullOrEmpty( result.ErrorMessage ) )
            {
                m.Error().Send( result.ErrorMessage );
            }
            else
            {
                m.Trace().Send( "Set Yodii.Script message html" );
                SetHtmlBody( result.Text );
            }

            m.Debug().Send( result.Script );

            return result;
        }
    }
}
