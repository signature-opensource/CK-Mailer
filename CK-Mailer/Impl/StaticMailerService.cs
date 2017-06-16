using CK.Core;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class StaticMailerService
    {
        public static Task SendMailAsync( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            return SendMailAsync( m, message, new MailKitClientProvider( options ) );
        }

        public static async Task SendMailAsync( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            var options = provider.Options;

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

        private static async Task InnerSendMailAsync( IActivityMonitor m, MimeMessage message, IMailKitClientProvider provider )
        {
            var client = await provider.GetClientAsync();

            await client.SendAsync( message ).ConfigureAwait( false );
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
