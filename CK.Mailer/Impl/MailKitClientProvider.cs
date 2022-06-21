using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Diagnostics;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Mailer
{
    public class MailKitClientProvider : IMailKitClientProvider
    {
        public MailKitOptions Options { get; private set; }

        public MailKitClientProvider( MailKitOptions options )
        {
            Options = options;
        }


        public async Task<SmtpClient> GetClientAsync( IActivityMonitor m )
        {
            var s = new SmtpClient();

             m.Info( $"Connecting to SMTP async {Options.Host}:{Options.Port}" );

            s.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
            await s.ConnectAsync( Options.Host,
                                            Options.Port,
                                            Options.UseSSL ).ConfigureAwait( false );

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            s.AuthenticationMechanisms.Remove( "XOAUTH2" );

            if( !String.IsNullOrEmpty( Options.User ) && !String.IsNullOrEmpty( Options.Password ) )
            {
                m.Info( $"Authenticating..." );

                await s.AuthenticateAsync( Options.User, Options.Password )
                    .ConfigureAwait( false );

                m.Info( $"Authenticated!" );
            }
            return s;
        }
    }
}
