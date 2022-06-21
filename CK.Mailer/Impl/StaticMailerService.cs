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
        public static Task SendMailAsync( IActivityMonitor m,
                                          MailKitOptions options,
                                          MimeMessage message,
                                          ITransferProgress progress = null,
                                          CancellationToken cancellationToken = default )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, new MailKitClientProvider( options ), message, progress, cancellationToken );
        }

        public static Task SendMailAsync( IActivityMonitor m,
                                          MailKitOptions options,
                                          FormatOptions formatOptions,
                                          MimeMessage message,
                                          ITransferProgress progress = null,
                                          CancellationToken cancellationToken = default )
        {
            if( options == null ) throw new ArgumentNullException( nameof( options ) );

            return SendMailAsync( m, new MailKitClientProvider( options ), formatOptions, message, progress, cancellationToken );
        }

        public static async Task SendMailAsync( IActivityMonitor m,
                                                IMailKitClientProvider provider,
                                                MimeMessage message,
                                                ITransferProgress progress = null,
                                                CancellationToken cancellationToken = default )
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
                using var client = await provider.GetClientAsync( m );
                await client.SendAsync( message, cancellationToken, progress ).ConfigureAwait( false );
            }
        }

        public static async Task SendMailAsync( IActivityMonitor m,
                                                IMailKitClientProvider provider,
                                                FormatOptions formatOptions,
                                                MimeMessage message,
                                                ITransferProgress progress = null,
                                                CancellationToken cancellationToken = default )
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
                using var client = await provider.GetClientAsync( m );
                await client.SendAsync( formatOptions, message, cancellationToken, progress ).ConfigureAwait( false );
            }
        }


        static void ProcessMessage( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            m.Info( $"Send Email, subject: {message.Subject}" );

            //set the DefaultSenderEmail when the From message collection is empty 
            if( !message.From.Any() )
            {
                if( String.IsNullOrWhiteSpace( options.DefaultSenderEmail ) )
                {
                    throw new InvalidOperationException( "'From' field is missing: 'DefaultSenderEmail' must be specified in Configuration." );
                }
                if( !String.IsNullOrEmpty( options.DefaultSenderName ) )
                {
                    message.From.Add( new MailboxAddress( options.DefaultSenderName, options.DefaultSenderEmail ) );
                }
                else
                {
                    message.From.Add( MailboxAddress.Parse( options.DefaultSenderEmail ) );
                }
            }

            if( message.From.Any() ) m.Info( $"From: {message.From}" );
            if( message.ResentFrom.Any() ) m.Info( $"ResentFrom: {message.ResentFrom}" );

            if( message.To.Any() ) m.Info( $"To: {message.To}" );
            if( message.ResentTo.Any() ) m.Info( $"ResentTo: {message.ResentTo}" );

            if( message.ReplyTo.Any() ) m.Info( $"ReplyTo: {message.ReplyTo}" );
            if( message.ResentReplyTo.Any() ) m.Info( $"ResentReplyTo: {message.ResentReplyTo}" );

            if( message.Cc.Any() ) m.Info( $"Cc: {message.Cc}"  );
            if( message.ResentCc.Any() ) m.Info( $"ResentCc: {message.ResentCc}" );

            if( message.Bcc.Any() ) m.Info( $"Bcc: {message.Bcc}" );
            if( message.ResentBcc.Any() ) m.Info( $"ResentBcc: {message.ResentBcc}" );

        }

        private static void WriteInThePickupDirectory( IActivityMonitor m, MimeMessage message, MailKitOptions options )
        {
            if( String.IsNullOrEmpty( options.PickupDirectoryPath ) ) throw new InvalidOperationException( "If the PickupDirectory option is used, the PickupDirectoryPath must be specified" );

            if( !Directory.Exists( options.PickupDirectoryPath ) )
            {
                using( m.OpenInfo( $"Creating PickupDirectory: {options.PickupDirectoryPath}" ) )
                {
                    try
                    {
                        Directory.CreateDirectory( options.PickupDirectoryPath );
                    }
                    catch( Exception ex )
                    {
                        m.Error( ex );
                    }
                }
            }

            var eml = $"{Guid.NewGuid()}.eml";
            var path = Path.Combine( options.PickupDirectoryPath, eml );
            using( m.OpenInfo( $"Creating file: '{eml}' in pickup directory {options.PickupDirectoryPath}." ) )
            {
                try
                {
                    using( var data = File.CreateText( path ) )
                    {
                        message.WriteTo( data.BaseStream );
                    }
                }
                catch( Exception ex )
                {
                    m.Error( ex );
                }
            }
        }

    }
}
