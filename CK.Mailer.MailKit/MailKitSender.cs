using CK.Core;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer.MailKit;

public class MailKitSender : IEmailSender
{
    readonly MailKitSenderOptions _options;

    public MailKitSender( MailKitSenderOptions options )
    {
        _options = options;
    }

    public Task<SendResponse> SendAsync( IActivityMonitor monitor,
                                         SimpleEmail email,
                                         CancellationToken token = default )
    {
        return SendAsync( monitor, email.GetMimeMessage(), token );
    }

    public async Task<SendResponse> SendAsync( IActivityMonitor monitor,
                                               MimeMessage email,
                                               CancellationToken token = default )
    {
        var response = new SendResponse();

        if( token.IsCancellationRequested )
        {
            response.ErrorMessages.Add( "Message was cancelled by cancellation token." );
            return response;
        }

        try
        {
            if( _options.UsePickupDirectory )
            {
                response = await SaveToPickupDirectoryAsync( monitor, email, _options.PickupDirectory, token );
            }

            if( !_options.SendEmail )
            {
                return response;
            }

            using var client = new SmtpClient();
            if( _options.SocketOptions.HasValue )
            {
                await client.ConnectAsync( _options.Host, _options.Port, _options.SocketOptions.Value, token );
            }
            else
            {
                await client.ConnectAsync( _options.Host, _options.Port, _options.UseSsl, token );
            }

            // Note: Only needed if the SMTP server requires authentication.
            if( _options.RequiresAuthentication )
            {
                await client.AuthenticateAsync( _options.User, _options.Password, token );
            }

            await client.SendAsync( email, token );
            await client.DisconnectAsync( true, token );

            response.MessageId = response.MessageId is null
                ? email.MessageId
                : $"{response.MessageId},{email.MessageId}";
        }
        catch( Exception ex )
        {
            monitor.Error( ex );
            response.ErrorMessages.Add( ex.Message );
        }

        return response;
    }

    public static Task<SendResponse> SaveToPickupDirectoryAsync( IActivityMonitor monitor,
                                                                 SimpleEmail message,
                                                                 NormalizedPath pickupDirectory,
                                                                 CancellationToken token = default )
    {
        return SaveToPickupDirectoryAsync( monitor, message.GetMimeMessage(), pickupDirectory, token );
    }

    static async Task<SendResponse> SaveToPickupDirectoryAsync( IActivityMonitor monitor,
                                                                MimeMessage message,
                                                                NormalizedPath pickupDirectory,
                                                                CancellationToken token = default )
    {
        var path = pickupDirectory.AppendPart( Guid.NewGuid().ToString() + ".eml" );
        var response = new SendResponse();

        try
        {
            using var fs = File.Create( path );
            await message.WriteToAsync( fs, token );

            monitor.Info( $"Email successfully saved in '{path}'." );
            response.MessageId = path.LastPart;
        }
        catch( Exception ex )
        {
            monitor.Error( ex );
            response.ErrorMessages.Add( ex.Message );
        }

        return response;
    }
}
