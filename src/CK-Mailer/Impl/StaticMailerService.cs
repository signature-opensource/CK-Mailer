using CK.Core;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class StaticMailerService
    {
        public static Task SendMailAsync( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, message, new MailKitClientProvider( options ) );
        }

        public static Task SendMailAsync( IActivityMonitor m, BasicMailModel message, MailKitOptions options )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, message.ToMimeMessage(), new MailKitClientProvider( options ) );
        }

        public static Task SendMailAsync( IActivityMonitor m, BasicMailModel message, IMailKitClientProvider provider )
        {
            return SendMailAsync( m, message.ToMimeMessage(), provider );
        }

        public static async Task SendMailAsync( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            var options = provider.Options;

            ProcessMessage( m, message, options );

            if( options.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( options.UsePickupDirectory.Value )
            {
                WriteInThePickupDirectory( m, message, options );
            }

            if( options.SendMails )
            {
                await InnerSendMailAsync( m, message, provider );
            }
        }

        public static void SendMail( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            SendMail( m, message, new MailKitClientProvider( options ) );
        }

        public static void SendMail( IActivityMonitor m, BasicMailModel message, MailKitOptions options )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            SendMail( m, message.ToMimeMessage(), new MailKitClientProvider( options ) );
        }

        public static void SendMail( IActivityMonitor m, BasicMailModel message, IMailKitClientProvider provider )
        {
            SendMail( m, message.ToMimeMessage(), provider );
        }

        public static void SendMail( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            var options = provider.Options;

            ProcessMessage( m, message, options );

            if( options.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( options.UsePickupDirectory.Value )
            {
                try
                {
                    WriteInThePickupDirectory( m, message, options );
                }
                catch( Exception ex )
                {
                    //todo monitor send an email ? another package ?
                    m.Error().Send( "An error occured during WriteInThePickupDirectory method" );
                    m.Error().Send( ex );
                }
            }

            if( options.SendMails )
            {
                InnerSendMail( m, message, provider );
            }
        }

        private static void ProcessMessage( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            m.Info().Send( "Send Email, subject: {0}", message.Subject );

            //set the DefaultSenderEmail when the From message collection is empty 
            if( !message.From.Any() )
            {
                m.Info().Send( "Add default Email sender, subject: {0}", options.DefaultSenderName );
                if( !String.IsNullOrEmpty( options.DefaultSenderName ) )
                {
                    message.From.Add( new MailboxAddress( options.DefaultSenderName, options.DefaultSenderEmail ) );
                }
                else
                {
                    message.From.Add( new MailboxAddress( options.DefaultSenderEmail ) );
                }
            }


            if( message.From.Any() ) m.Info().Send( "From: {0}", message.From );
            if( message.ResentFrom.Any() ) m.Info().Send( "ResentFrom: {0}", message.ResentFrom );

            if( message.To.Any() ) m.Info().Send( "To: {0}", message.To );
            if( message.ResentTo.Any() ) m.Info().Send( "ResentTo: {0}", message.ResentTo );

            if( message.ReplyTo.Any() ) m.Info().Send( "ReplyTo: {0}", message.ReplyTo );
            if( message.ResentReplyTo.Any() ) m.Info().Send( "ResentReplyTo: {0}", message.ResentReplyTo );

            if( message.Cc.Any() ) m.Info().Send( "Cc: {0}", message.Cc );
            if( message.ResentCc.Any() ) m.Info().Send( "ResentCc: {0}", message.ResentCc );

            if( message.Bcc.Any() ) m.Info().Send( "Bcc: {0}", message.Bcc );
            if( message.ResentBcc.Any() ) m.Info().Send( "ResentBcc: {0}", message.ResentBcc );

        }

        private static async Task InnerSendMailAsync( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            m.Info().Send( "Getting Smtp Client" );

            var client = await provider.GetClientAsync();

            m.Info().Send( "Sending Email" );

            await client.SendAsync( message ).ConfigureAwait( false );

            m.Info().Send( "Email sent" );
        }

        private static void InnerSendMail( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            m.Info().Send( "Getting Smtp Client" );

            var client = provider.GetClient();

            m.Info().Send( "Sending Email" );

            client.Send( message );

            m.Info().Send( "Email sent" );
        }

        private static void WriteInThePickupDirectory( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            if( String.IsNullOrEmpty( options.PickupDirectoryPath ) ) throw new InvalidOperationException( "If the PickupDirectory option is used, the PickupDirectoryPath must be specified" );

            if( !Directory.Exists( options.PickupDirectoryPath ) )
            {
                try
                {
                    Directory.CreateDirectory( options.PickupDirectoryPath );
                }
                catch( Exception ex )
                {
                    m.Error().Send( "Can't create PickupDirectory" );
                    m.Error().Send( ex );
                }
            }

            var path = Path.Combine( options.PickupDirectoryPath, $"{Guid.NewGuid().ToString()}.eml" );

            using( var data = File.CreateText( path ) )
            {
                message.WriteTo( data.BaseStream );
                m.Info().Send( "WriteTo: {0}", path );
            }
        }
    }
}
