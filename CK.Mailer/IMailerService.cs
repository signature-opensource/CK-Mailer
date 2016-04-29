using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Mailer
{
    public interface IMailerService
    {
        /// <summary>
        /// Low-level API to send a mail.
        /// </summary>
        /// <param name="logger">An activityLogger to log the operation</param>
        /// <param name="mailParams">Mail parameters</param>
        void SendFinalMail( IActivityMonitor logger, MailParams mailParams );
        
        /// <summary>
        /// Send a templated mail to a list of recipients
        /// </summary>
        /// <typeparam name="T">Model for this mail, must implement IMailConfigurator<T></typeparam>
        /// <param name="model">model instance that contain data for the mail rendering and the configurator of this mail</param>
        /// <param name="mailKey">key to lookup for the mail body and subject</param>
        /// <param name="recipients">List of recipients</param>
        void SendMail<T, Q>(T model, Q mailKey, params Recipient[] recipients)
            where T : IMailConfigurator<T>
            where Q : IMailTemplateKey;

        /// <summary>
        /// Send a templated mail to a list of recipients
        /// </summary>
        /// <typeparam name="T">Model for this mail, must implement IMailConfigurator<T></typeparam>
        /// <param name="model">model instance that contain data for the mail rendering and the configurator of this mail</param>
        /// <param name="mailKey">key to lookup for the mail body and subject</param>
        /// <param name="recipients">recipients model that contain also CC and CCI fields</param>
        void SendMail<T, Q>(T model, Q mailKey, RecipientModel recipientsAndCCAndCCI)
            where T : IMailConfigurator<T>
            where Q : IMailTemplateKey;

        /// <summary>
        /// Send a templated mail to a list of recipients
        /// </summary>
        /// <typeparam name="T">Model for this mail</typeparam>
        /// <param name="model">model instance that contain data for the mail rendering</param>
        /// <param name="mailKey">key to lookup for the mail body and subject</param>
        /// <param name="configurator">configurator instance for this mail</param>
        /// <param name="recipients">List of recipients</param>
        void SendMail<T, Q>(T model, Q mailKey, IMailConfigurator<T> configurator, params Recipient[] recipients) 
            where Q : IMailTemplateKey;

        /// <summary>
        /// Send a templated mail to a list of recipients
        /// </summary>
        /// <typeparam name="T">Model for this mail</typeparam>
        /// <param name="model">model instance that contain data for the mail rendering</param>
        /// <param name="mailKey">key to lookup for the mail body and subject</param>
        /// <param name="configurator">configurator instance for this mail</param>
        /// <param name="recipientsAndCCAndCCI">recipients model that contain also CC and CCI fields</param>
        void SendMail<T,Q>( T model, Q mailKey, IMailConfigurator<T> configurator, RecipientModel recipientsAndCCAndCCI )
            where Q : IMailTemplateKey;
    }
}