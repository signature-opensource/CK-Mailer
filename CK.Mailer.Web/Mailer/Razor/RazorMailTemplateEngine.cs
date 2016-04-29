using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using CK.Core;
using RazorEngine;

namespace CK.Mailer
{
    public class RazorMailTemplateEngine : IMailTemplateEngine<RazorMailTemplateKey>
    {
        readonly IPhysicalPathProvider _physPathProvider;
        readonly IActivityMonitor _activityLogger;

        public RazorMailTemplateEngine()
            : this( new HostingEnvironmentPhysicalPathProvider(), new ActivityMonitor() )
        {
        }

        public RazorMailTemplateEngine( IPhysicalPathProvider physPathProvider )
            : this( physPathProvider, new ActivityMonitor() )
        {
        }

        public RazorMailTemplateEngine( IActivityMonitor activityLogger )
            : this( new HostingEnvironmentPhysicalPathProvider(), activityLogger )
        {
        }

        public RazorMailTemplateEngine( IPhysicalPathProvider physPathProvider, IActivityMonitor activityLogger )
        {
            _physPathProvider = physPathProvider;
            _activityLogger = activityLogger;
        }

        public virtual bool Exist( RazorMailTemplateKey mailKey )
        {
            if( mailKey != null )
            {
                string virtualPath = mailKey.GetVirtualPath();
                return File.Exists( _physPathProvider.MapPath( virtualPath ) );
            }
            return false;
        }

        public virtual string GetBody<T>( T model, RazorMailTemplateKey mailKey )
        {
            if( mailKey != null )
            {
                string virtualPath = mailKey.GetVirtualPath();
                string template= System.IO.File.ReadAllText( _physPathProvider.MapPath( virtualPath ) );
                string razor = Razor.Parse<T>( template, model, mailKey.Key );
                return HttpUtility.HtmlDecode( razor );
            }
            return null;
        }

        //public void SendMail<T>( T model, IMailTemplateKey mailKey, params Recipient[] recipients ) where T : IHaveSubject
        //{
        //    using( var group = _activityLogger.OpenInfo().Send( "Sending mail {0} to {1}", mailKey.Key, String.Join( ", ", recipients.Select( e => e.EmailAddress ) ) ) )
        //    {
        //        string body = GetBody( model, mailKey );
        //        if( body != null )
        //        {
        //            SmtpClient client = new SmtpClient();
        //            try
        //            {
        //                var mm = new MailMessage
        //                {
        //                    Subject = model.Subject,
        //                    Body = body,
        //                    IsBodyHtml = true
        //                };
        //                foreach( var recipient in recipients ) mm.To.Add( new MailAddress( recipient.EmailAddress, recipient.DisplayName ) );
        //                client.Send( mm );
        //                _activityLogger.Info().Send("Mail sent" );
        //            }
        //            catch( SmtpFailedRecipientException e )
        //            {
        //                _activityLogger.Warn().Send(e );
        //            }
        //            catch( SmtpException smtpException )
        //            {
        //                _activityLogger.Error().Send(smtpException );
        //            }
        //            catch( Exception exception )
        //            {
        //                _activityLogger.Error().Send(exception );
        //                throw exception;
        //            }
        //            finally
        //            {
        //                if( client != null ) client.Dispose();
        //            }
        //        }
        //        else
        //        {
        //            _activityLogger.Warn().Send("Template not found" );
        //        }
        //    }
        //}
    }
}