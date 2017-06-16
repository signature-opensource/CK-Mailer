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
        public static async Task SendMailAsync( IActivityMonitor m, MimeMessage message, SmtpOptions smtpOptions )
        {
            if( smtpOptions.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( smtpOptions.UsePickupDirectory.Value )
            {
                WriteInThePickupDirectory( m, message, smtpOptions );
            }

            if( smtpOptions.SendMails )
            {
                await InnerSendMailAsync( m, message, smtpOptions );
            }
        }

        private static async Task InnerSendMailAsync( IActivityMonitor m, MimeMessage message, SmtpOptions smtpOptions )
        {
            using( var client = new SmtpClient() )
            {
                client.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
                await client.ConnectAsync(
                    smtpOptions.Host,
                    smtpOptions.Port,
                    smtpOptions.UseSsl ).ConfigureAwait( false );

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove( "XOAUTH2" );

                if( !String.IsNullOrEmpty( smtpOptions.User ) && !String.IsNullOrEmpty( smtpOptions.Password ) )
                {
                    await client.AuthenticateAsync( smtpOptions.User, smtpOptions.Password )
                        .ConfigureAwait( false );
                }
                await client.SendAsync( message ).ConfigureAwait( false );

                await client.DisconnectAsync( true ).ConfigureAwait( false );
            }
        }

        private static void WriteInThePickupDirectory( IActivityMonitor m, MimeMessage message, SmtpOptions smtpOptions )
        {
            if( !string.IsNullOrWhiteSpace( smtpOptions.PickupDirectory ) && !Directory.Exists( smtpOptions.PickupDirectory ) )
            {
                try
                {
                    Directory.CreateDirectory( smtpOptions.PickupDirectory );
                }
                catch( Exception ex )
                {
                    m.Error().Send( "Can't create PickupDirectory" );
                    m.Error().Send( ex );
                }
            }

            var path = Path.Combine( smtpOptions.PickupDirectory, $"{Guid.NewGuid().ToString()}.eml" );

            using( var data = File.CreateText( path ) )
            {
                message.WriteTo( data.BaseStream );
            }
        }
    }
}
