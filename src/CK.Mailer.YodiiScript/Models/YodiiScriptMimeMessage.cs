using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using CK.Mailer.YodiiScript;
using Yodii.Script;
using CK.Core;

namespace CK.Mailer.Razor
{
    public class YodiiScriptMimeMessage : SimpleMimeMessage, IYodiiScriptMimeMessage
    {
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

        public TemplateEngine.Result SetBodyFromYodiiScriptString<T>( IActivityMonitor m, T model, string content )
        {
            m.Trace().Send( "Create Yodii.Script.GlobalContext" );

            var c = new GlobalContext();

            m.Trace().Send( "Register Yodii.Script template Model" );

            c.Register( "Model", model );

            m.Trace().Send( "Create Yodii.Script.TemplateEngine" );

            var e = new TemplateEngine( c );

            m.Trace().Send( "Process Yodii.Script template" );

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
