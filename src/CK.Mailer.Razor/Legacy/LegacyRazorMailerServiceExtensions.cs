using CK.Core;
using CK.Mailer.Razor;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class LegacyRazorMailerServiceExtensions
    {
        [Obsolete( "Use to legacy compatibility" )]
        public static void SendMail<T, Q>( this IRazorMailerService @this, T model, Q mailKey, params Recipient[] recipients )
            where T : IMailConfigurator<T>
            where Q : IMailTemplateKey
        {
            SendMail( @this, model, mailKey, model as IMailConfigurator<T>, recipients );
        }

        [Obsolete( "Use to legacy compatibility" )]
        public static void SendMail<T, Q>( this IRazorMailerService @this, T model, Q mailKey, IMailConfigurator<T> configurator, params Recipient[] recipients )
            where Q : IMailTemplateKey
        {
            RecipientModel m = new RecipientModel();
            m.Recipients.AddRange( recipients );
            SendMail( @this, model, mailKey, configurator, m );
        }

        [Obsolete( "Use to legacy compatibility" )]
        public static void SendMail<T, Q>( this IRazorMailerService @this, T model, Q mailKey, RecipientModel recipientsAndCCAndCCI )
            where T : IMailConfigurator<T>
            where Q : IMailTemplateKey
        {
            SendMail( @this, model, mailKey, model as IMailConfigurator<T>, recipientsAndCCAndCCI );
        }

        [Obsolete( "Use to legacy compatibility" )]
        public static void SendMail<T, Q>( this IRazorMailerService @this, T model, Q mailKey, IMailConfigurator<T> configurator, RecipientModel recipientsAndCCAndCCI )
            where Q : IMailTemplateKey
        {
            Recipients m = new Recipients();

            m.To.AddRange(
                recipientsAndCCAndCCI
                    .Recipients
                    .Select( x => new MailboxAddress( x.DisplayName, x.EmailAddress ) ) );

            m.Cc.AddRange(
                recipientsAndCCAndCCI
                    .CC
                    .Select( x => new MailboxAddress( x.DisplayName, x.EmailAddress ) ) );

            m.Bcc.AddRange(
                recipientsAndCCAndCCI
                    .BCC
                    .Select( x => new MailboxAddress( x.DisplayName, x.EmailAddress ) ) );

            var razorModel = new RazorMailModel<T>( model )
            {
                Recipients = m,
                Subject = configurator.GetSubject( model )
            };

            @this.Send( new ActivityMonitor(), mailKey.Key, razorModel );
        }
    }
}
