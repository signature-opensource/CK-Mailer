using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Mailer
{
    public class DefaultMailerService : MailerServiceBase
    {
        private static Regex _scriptTags = new Regex( "<script.*?>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled );

        readonly IMailTemplateEngineFactory _engineFactory;
        readonly IActivityMonitor _activityLogger;

        public DefaultMailerService()
            : this( new ActivityMonitor() )
        {
        }

        public DefaultMailerService( IActivityMonitor activityLogger )
            : this( new DefaultMailTemplateEngineFactory(), activityLogger )
        {
            _engineFactory.AddEngine( new RazorMailTemplateEngine() ); // Add razor engine by default
        }

        public DefaultMailerService( IMailTemplateEngineFactory engineFactory )
            : this( engineFactory, new ActivityMonitor() )
        {
        }

        public DefaultMailerService( IMailTemplateEngineFactory engineFactory, IActivityMonitor activityLogger )
        {
            _engineFactory = engineFactory;
            _activityLogger = activityLogger;
        }

        public override void SendMail<T, Q>( T model, Q mailKey, IMailConfigurator<T> configurator, RecipientModel recipientsAndCCAndCCI )
        {
            using( var group = _activityLogger.OpenInfo().Send( "Try send mail {0} to {1}", mailKey.Key, recipientsAndCCAndCCI ) )
            {
                IMailTemplateEngine<Q> engine = _engineFactory.GetEngine( mailKey );
                if( engine == null )
                {
                    throw new ArgumentNullException( "There is no engine corresponding to the current key in the engine factory" );
                }

                if( !engine.Exist( mailKey ) )
                {
                    throw new InvalidOperationException( "The key doesn't exist for the engine" );
                }

                string body = engine.GetBody( model, mailKey );

                if( string.IsNullOrWhiteSpace( body ) )
                {
                    throw new CKException( "The engine should not return an null or empty body" );
                }

                MailParams mailParam = new MailParams();
                // Use good recipients
                if( recipientsAndCCAndCCI != null ) mailParam.Recipients = recipientsAndCCAndCCI;

                // Retreve good title
                mailParam.Subject = configurator.GetSubject( model );

                // Use good body
                mailParam.Body = body;

                // Let configurator do his work
                configurator.ConfigureMail( model, mailParam );

                // SendFinalMail will check all the mail constraints
                SendFinalMail( _activityLogger, mailParam );
            }
        }

        protected override void PrepareBody( MailParams mailParams, MailMessage m )
        {
            using( var group = _activityLogger.OpenInfo().Send( "Prepare body for mail with subject : {0}", mailParams.Subject ) )
            {
                string body = BareLF.Replace( mailParams.Body, "\r\n" );
                if( mailParams.IsBodyHtml == true )
                {
                    _activityLogger.Info().Send( "Mail body is HTML and text content (text version is HtmlToText version)" );
                    body = _scriptTags.Replace( body, "" ); // Remove scripts tags !

                    m.Body = WebUtil.HtmlToText( body, true );
                    m.IsBodyHtml = false; //Body is not in HTML, it's the plain text version

                    m.BodyEncoding = mailParams.BodyEncoding; // Encoding is not used at all

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString( body, new ContentType( MediaTypeNames.Text.Html ) { CharSet = mailParams.BodyEncoding.WebName } );
                    m.AlternateViews.Add( htmlView );
                }
                else
                {
                    _activityLogger.Info().Send( "Mail body is Text only content" );
                    m.IsBodyHtml = false;
                    m.Body = body;
                    m.BodyEncoding = mailParams.BodyEncoding;
                }
            }
        }
    }
}