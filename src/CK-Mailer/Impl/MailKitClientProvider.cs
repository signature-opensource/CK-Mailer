using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public class MailKitClientProvider : IMailKitClientProvider
    {
        public MailKitOptions Options { get; private set; }

        public MailKitClientProvider( MailKitOptions options )
        {
            Options = options;
        }

        public SmtpClient GetClient()
        {
            if( _isDisposed ) throw new ObjectDisposedException( nameof( MailKitClientProvider ) );

            //if( _smtpClient != null ) return _smtpClient;

            _smtpClient = new SmtpClient();

            ConfigurationSmtp();

            return _smtpClient;
        }

        public async Task<SmtpClient> GetClientAsync()
        {
            if( _isDisposed ) throw new ObjectDisposedException( nameof( MailKitClientProvider ) );

            //if( _smtpClient != null ) return _smtpClient;

            _smtpClient = new SmtpClient();

            await ConfigurationSmtpAsync();

            return _smtpClient;
        }

        private async Task ConfigurationSmtpAsync()
        {
            Debug.Assert( _smtpClient != null );

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
                await _smtpClient.AuthenticateAsync( Options.User, Options.Password )
                    .ConfigureAwait( false );
            }
        }

        private void ConfigurationSmtp()
        {
            Debug.Assert( _smtpClient != null );

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
                _smtpClient.Authenticate( Options.User, Options.Password );
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
