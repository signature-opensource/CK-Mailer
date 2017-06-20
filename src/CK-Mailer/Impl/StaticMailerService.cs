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
                WriteInThePickupDirectory( m, message, options );
            }

            if( options.SendMails )
            {
                InnerSendMail( m, message, provider );
            }
        }

        private static void ProcessMessage( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            //set the DefaultSenderEmail when the From message collection is empty 
            if( !message.From.Any() )
            {
                if( !String.IsNullOrEmpty( options.DefaultSenderName ) )
                {
                    message.From.Add( new MailboxAddress( options.DefaultSenderName, options.DefaultSenderEmail ) );
                }
                else
                {
                    message.From.Add( new MailboxAddress( options.DefaultSenderEmail ) );
                }
            }
        }

        private static async Task InnerSendMailAsync( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            var client = await provider.GetClientAsync();

            await client.SendAsync( message ).ConfigureAwait( false );
        }

        private static void InnerSendMail( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            var client = provider.GetClient();

            client.Send( message );
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
            }
        }
    }
}
