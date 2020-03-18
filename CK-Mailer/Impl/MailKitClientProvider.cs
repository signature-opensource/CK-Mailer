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

        public SmtpClient GetClient( IActivityMonitor m )
        {
            if( _isDisposed ) throw new ObjectDisposedException( nameof( MailKitClientProvider ) );

            //if( _smtpClient != null ) return _smtpClient;

            _smtpClient = new SmtpClient();

            ConfigurationSmtp( m );

            return _smtpClient;
        }

        public async Task<SmtpClient> GetClientAsync( IActivityMonitor m )
        {
            if( _isDisposed ) throw new ObjectDisposedException( nameof( MailKitClientProvider ) );

            //if( _smtpClient != null ) return _smtpClient;

            _smtpClient = new SmtpClient();

            await ConfigurationSmtpAsync( m );

            return _smtpClient;
        }

        private async Task ConfigurationSmtpAsync( IActivityMonitor m )
        {
            Debug.Assert( _smtpClient != null );

            m.Info().Send( $"Connecting to SMTP async {Options.Host}:{Options.Port}" );

            _smtpClient.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
            await _smtpClient.ConnectAsync(
                Options.Host,
                Options.Port,
                Options.UseSSL ).ConfigureAwait( false );

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            _smtpClient.AuthenticationMechanisms.Remove( "XOAUTH2" );

            if( !String.IsNullOrEmpty( Options.User ) && !String.IsNullOrEmpty( Options.Password ) )
            {
                m.Info().Send( $"Authenticating..." );

                await _smtpClient.AuthenticateAsync( Options.User, Options.Password )
                    .ConfigureAwait( false );

                m.Info().Send( $"Authenticated!" );
            }
        }

        private void ConfigurationSmtp( IActivityMonitor m )
        {
            Debug.Assert( _smtpClient != null );

            m.Info().Send( $"Connecting to SMTP {Options.Host}:{Options.Port}" );

            _smtpClient.ServerCertificateValidationCallback = ( s, c, h, e ) => true;
            _smtpClient.Connect(
                Options.Host,
                Options.Port,
                Options.UseSSL );

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            _smtpClient.AuthenticationMechanisms.Remove( "XOAUTH2" );

            if( !String.IsNullOrEmpty( Options.User ) && !String.IsNullOrEmpty( Options.Password ) )
            {
                m.Info().Send( $"Authenticating..." );

                _smtpClient.Authenticate( Options.User, Options.Password );

                m.Info().Send( $"Authenticated!" );
            }
        }

        private SmtpClient _smtpClient;

        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls

        public void Dispose()
        {
            if( !_isDisposed )
            {
                _smtpClient.Disconnect( true );
                _smtpClient.Dispose();

                _isDisposed = true;
            }
        }
        #endregion
    }
}
