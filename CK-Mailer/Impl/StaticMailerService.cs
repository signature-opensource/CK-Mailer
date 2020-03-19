using CK.Core;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public static class StaticMailerService
    {
        //public static Task SendMailAsync( IActivityMonitor m, SimpleMimeMessage message, MailKitOptions options )
        //{
        //    if( options == null ) throw new ArgumentNullException( nameof( options ) );

        //    return SendMailAsync( m, message, new MailKitClientProvider( options ) );
        //}

        //public static Task SendMailAsync( IActivityMonitor m, SimpleMimeMessage message, IMailKitClientProvider provider )
        //{
        //    return SendMailAsync( m, message, provider );
        //}

        public static Task SendMailAsync( IActivityMonitor m, MailKitOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, new MailKitClientProvider( options ), message, cancellationToken, progress );
        }

        public static Task SendMailAsync( IActivityMonitor m, MailKitOptions options, FormatOptions formatOptions, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, new MailKitClientProvider( options ), formatOptions, message, cancellationToken, progress );
        }

        public static async Task SendMailAsync( IActivityMonitor m, IMailKitClientProvider provider, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            if( String.IsNullOrEmpty( message.HtmlBody ) && String.IsNullOrEmpty( message.TextBody ) ) throw new InvalidOperationException( "The message body must be set" );

            var options = provider.Options;

            ProcessMessage( m, message, options );

            if( options.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( options.UsePickupDirectory.Value )
            {
                WriteInThePickupDirectory( m, message, options );
            }

            if( options.SendMails )
            {
                await InnerSendMailAsync( m, provider, message, cancellationToken, progress );
            }
        }

        public static async Task SendMailAsync( IActivityMonitor m, IMailKitClientProvider provider, FormatOptions formatOptions, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            if( String.IsNullOrEmpty( message.HtmlBody ) && String.IsNullOrEmpty( message.TextBody ) ) throw new InvalidOperationException( "The message body must be set" );

            var options = provider.Options;

            ProcessMessage( m, message, options );

            if( options.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( options.UsePickupDirectory.Value )
            {
                WriteInThePickupDirectory( m, message, options );
            }

            if( options.SendMails )
            {
                await InnerSendMailAsync( m, provider, formatOptions, message, cancellationToken, progress );
            }
        }



        //public static void SendMail( IActivityMonitor m, SimpleMimeMessage message, MailKitOptions options )
        //{
        //    if( options == null ) throw new ArgumentNullException( nameof( options ) );

        //    SendMail( m, message, new MailKitClientProvider( options ) );
        //}

        //public static void SendMail( IActivityMonitor m, SimpleMimeMessage message, IMailKitClientProvider provider )
        //{
        //    SendMail( m, message, provider );
        //}

        public static void SendMail( IActivityMonitor m, MailKitOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            SendMail( m, new MailKitClientProvider( options ), message, cancellationToken, progress );
        }

        public static void SendMail( IActivityMonitor m, MailKitOptions options, FormatOptions formatOptions, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            SendMail( m, new MailKitClientProvider( options ), formatOptions, message, cancellationToken, progress );
        }

        public static void SendMail( IActivityMonitor m, IMailKitClientProvider provider, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            if( String.IsNullOrEmpty( message.HtmlBody ) && String.IsNullOrEmpty( message.TextBody ) ) throw new InvalidOperationException( "The message body must be set" );

            var providerOptions = provider.Options;

            ProcessMessage( m, message, providerOptions );

            if( providerOptions.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( providerOptions.UsePickupDirectory.Value )
            {
                try
                {
                    WriteInThePickupDirectory( m, message, providerOptions );
                }
                catch( Exception ex )
                {
                    //todo monitor send an email ? another package ?
                    m.Error().Send( "An error occured during WriteInThePickupDirectory method" );
                    m.Error().Send( ex );
                }
            }

            if( providerOptions.SendMails )
            {
                InnerSendMail( m, provider, message, cancellationToken, progress );
            }
        }

        public static void SendMail( IActivityMonitor m, IMailKitClientProvider provider, FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            if( m == null ) throw new InvalidOperationException( "An ActivityMonitor must be provided." );
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( provider == null ) throw new ArgumentNullException( nameof( provider ) );

            if( String.IsNullOrEmpty( message.HtmlBody ) && String.IsNullOrEmpty( message.TextBody ) ) throw new InvalidOperationException( "The message body must be set" );

            var providerOptions = provider.Options;

            ProcessMessage( m, message, providerOptions );

            if( providerOptions.UsePickupDirectory == null ) throw new InvalidOperationException( "UsePickupDirectory configuration must be define." );
            if( providerOptions.UsePickupDirectory.Value )
            {
                try
                {
                    WriteInThePickupDirectory( m, message, providerOptions );
                }
                catch( Exception ex )
                {
                    //todo monitor send an email ? another package ?
                    m.Error().Send( "An error occured during WriteInThePickupDirectory method" );
                    m.Error().Send( ex );
                }
            }

            if( providerOptions.SendMails )
            {
                InnerSendMail( m, provider, options, message, cancellationToken, progress );
            }
        }



        private static void ProcessMessage( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            m.Info().Send( "Send Email, subject: {0}", message.Subject );

            //set the DefaultSenderEmail when the From message collection is empty 
            if( !message.From.Any() )
            {
                if( String.IsNullOrWhiteSpace( options.DefaultSenderEmail ) )
                    throw new InvalidOperationException( "DefaultSenderEmail must be defined, if you don't specify the from field." );

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
        private static void WriteInThePickupDirectory( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            if( String.IsNullOrEmpty( options.PickupDirectoryPath ) ) throw new InvalidOperationException( "If the PickupDirectory option is used, the PickupDirectoryPath must be specified" );

            if( !Directory.Exists( options.PickupDirectoryPath ) )
            {
                m.Info().Send( "PickupDirectory not found : {0}", options.PickupDirectoryPath );

                try
                {
                    m.Info().Send( "Create PickupDirectory" );
                    Directory.CreateDirectory( options.PickupDirectoryPath );
                }
                catch( Exception ex )
                {
                    m.Error().Send( "Can't create PickupDirectory" );
                    m.Error().Send( ex );
                }
            }

            m.Info().Send( "Writing mail to pickup directory" );

            var path = Path.Combine( options.PickupDirectoryPath, $"{Guid.NewGuid().ToString()}.eml" );

            using( var data = File.CreateText( path ) )
            {
                message.WriteTo( data.BaseStream );
                m.Info().Send( "WriteTo: {0}", path );
            }
        }



        private static Task<SmtpClient> InnerGetSmtpClientAsync( IActivityMonitor m, IMailKitClientProvider provider )
        {
            m.Info().Send( "Getting Smtp Client" );

            return provider.GetClientAsync( m );
        }

        private static SmtpClient InnerGetSmtpClient( IActivityMonitor m, IMailKitClientProvider provider )
        {
            m.Info().Send( "Getting Smtp Client" );

            return provider.GetClient( m );
        }



        public static void InnerSendMail( IActivityMonitor m, IMailKitClientProvider provider, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            var client = InnerGetSmtpClient( m, provider );

            m.Info().Send( "Sending Email" );

            client.Send( message, cancellationToken, progress );

            m.Info().Send( "Email sent" );
        }

        public static void InnerSendMail( IActivityMonitor m, IMailKitClientProvider provider, FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            var client = InnerGetSmtpClient( m, provider );

            m.Info().Send( "Sending Email" );

            client.Send( options, message, cancellationToken, progress );

            m.Info().Send( "Email sent" );
        }

        public static async Task InnerSendMailAsync( IActivityMonitor m, IMailKitClientProvider provider, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            var client = await InnerGetSmtpClientAsync( m, provider );

            m.Info().Send( "Sending Email" );

            await client.SendAsync( message, cancellationToken, progress ).ConfigureAwait( false );

            m.Info().Send( "Email sent" );
        }

        public static async Task InnerSendMailAsync( IActivityMonitor m, IMailKitClientProvider provider, FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default( CancellationToken ), ITransferProgress progress = null )
        {
            var client = await InnerGetSmtpClientAsync( m, provider );

            m.Info().Send( "Sending Email" );

            await client.SendAsync( options, message, cancellationToken, progress ).ConfigureAwait( false );

            m.Info().Send( "Email sent" );
        }
    }
}
