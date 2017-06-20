using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Mailer
{
    public abstract class MailerServiceBase : IMailerService
    {
        private MailerSection _section;

        public MailerServiceBase()
        {
            _section = ConfigurationManager.GetSection( "mailer" ) as MailerSection;
        }

        public static Regex BareLF = new Regex( @"(?<=[^\r])\n|\r(?=[^\n])", RegexOptions.Multiline | RegexOptions.Compiled );

        public virtual void SendMail<T, Q>(T model, Q mailKey, params Recipient[] recipients) 
            where T : IMailConfigurator<T> 
            where Q : IMailTemplateKey
        {
            RecipientModel m = new RecipientModel();
            m.Recipients.AddRange( recipients );
            SendMail( model, mailKey, model as IMailConfigurator<T>, m );
        }

        public virtual void SendMail<T, Q>(T model, Q mailKey, IMailConfigurator<T> configurator, params Recipient[] recipients)
            where Q : IMailTemplateKey
        {
            RecipientModel m = new RecipientModel();
            m.Recipients.AddRange( recipients );
            SendMail( model, mailKey, configurator, m );
        }

        public virtual void SendMail<T, Q>(T model, Q mailKey, RecipientModel recipientsAndCCAndCCI) 
            where T : IMailConfigurator<T> 
            where Q : IMailTemplateKey
        {
            SendMail( model, mailKey, model as IMailConfigurator<T>, recipientsAndCCAndCCI );
        }

        public abstract void SendMail<T,Q>(T model, Q mailKey, IMailConfigurator<T> configurator, RecipientModel recipientsAndCCAndCCI)
            where Q : IMailTemplateKey;

        /// <summary>
        /// Must set at least Body and IsBodyHtml on the MailMessage
        /// Must by BareLF prouf on textual and html body
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mailParams"></param>
        /// <param name="message"></param>
        protected abstract void PrepareBody( MailParams mailParams, MailMessage message );
        
        /// <summary>
        /// Send the mail defined by the MailParams
        /// </summary>
        /// <param name="logger">current logger</param>
        /// <param name="mailParams">definition of the mail to send</param>
        public virtual void SendFinalMail( IActivityMonitor logger, MailParams mailParams )
        {
            using( var group = logger.OpenInfo().Send( "Send final mail with subject : {0} to {1}", mailParams.Subject, mailParams.Recipients ) )
            {
                try
                {
                    using( MailMessage m = new MailMessage() )
                    {
                        AddRecipients( mailParams, m, logger );

                        //m.Subject = ToolkitUtil.RemoveDiacritics( ToolkitUtil.HtmlDecodeWithBr( mailParams.Subject, true ) ).Replace( '&', ' ' ).Replace( '%', ' ' );
                        m.Subject = mailParams.Subject;

                        PrepareBody( mailParams, m );

                        SafeAdd( m.Attachments, mailParams.Attachments, x => x );

                        // Just to be really sure
                        m.Body = BareLF.Replace( m.Body, "\r\n" );

                        if( mailParams.MustSend )
                        {
                            if( m.To.Count > 0 )
                            {
                                using( SmtpClient c = new SmtpClient() )
                                {
                                    c.Send( m );
                                }
                            }
                            else
                            {
                                logger.Info().Send("There is no more recipient in the mail. Mail will not be sent." );
                            }
                        }
                    }
                }
                catch( SmtpFailedRecipientException e )
                {
                    logger.Warn().Send(e );
                    throw;
                }
                catch( SmtpException smtpException )
                {
                    logger.Error().Send(smtpException );
                    throw;
                }
                catch( Exception exception )
                {
                    logger.Error().Send(exception );
                    throw;
                }
            }
        }

        private void AddRecipients( MailParams mailParams, MailMessage m, IActivityMonitor logger )
        {
            IEnumerable<WhiteListRecipient> wlc = null;
            if( _section != null && _section.IsRecipientWhiteListed == true && _section.WhiteList != null )
            {
                logger.Info().Send(string.Format( "The recipient address is white listed by config (IsRecipientWhiteListed: {0}, WhiteList.Count: {1})", _section.IsRecipientWhiteListed, _section.WhiteList.Count ) );
                wlc = _section.WhiteList.Cast<WhiteListRecipient>();
            }
            SafeAdd( m.To, mailParams.Recipients.Recipients, wlc );
            SafeAdd( m.CC, mailParams.Recipients.CC, wlc );
            SafeAdd( m.Bcc, mailParams.Recipients.BCC, wlc );
        }


        private void SafeAdd( MailAddressCollection collection, IEnumerable<Recipient> source, IEnumerable<WhiteListRecipient> wlc )
        {
            if( source != null && collection != null )
            {
                foreach( var item in source )
                {
                    if( wlc != null )
                    {
                        if( wlc.Where( x => x.Email == item.EmailAddress ).Any() )
                        {
                            collection.Add( new MailAddress( item.EmailAddress, item.DisplayName ) );
                        }
                    }
                    else
                    {
                        collection.Add( new MailAddress( item.EmailAddress, item.DisplayName ) );
                    }
                }
            }
        }

        private void SafeAdd<T,Q>( ICollection<T> collection, IEnumerable<Q> source, Func<Q, T> select )
        {
            if( source != null && select != null && collection != null )
            {
                foreach( var item in source )
                {
                    collection.Add( select( item ) );
                }
            }
        }
    }
}