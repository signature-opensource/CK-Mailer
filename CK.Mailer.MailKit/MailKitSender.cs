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

    public static async Task SaveToPickupDirectoryAsync( MimeMessage message, NormalizedPath pickupDirectory, CancellationToken token = default )
    {
        var path = pickupDirectory.AppendPart( Guid.NewGuid().ToString() + ".eml" );

        if( File.Exists( path ) ) return;

        using var fs = new FileStream( path, FileMode.CreateNew );
        await message.WriteToAsync( fs, token );
    }

    public async Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
    {
        var response = new SendResponse();

        if( token.IsCancellationRequested )
        {
            response.ErrorMessages.Add( "Message was cancelled by cancellation token." );
            return response;
        }

        var message = email.GetMimeMessage();

        try
        {
            if( _options.UsePickupDirectory )
            {
                await SaveToPickupDirectoryAsync( message, _options.MailPickupDirectory, token );
                return response;
            }

            using var client = new SmtpClient();
            if( _options.SocketOptions.HasValue )
            {
                await client.ConnectAsync(
                    _options.Server,
                    _options.Port,
                    _options.SocketOptions.Value,
                    token );
            }
            else
            {
                await client.ConnectAsync(
                    _options.Server,
                    _options.Port,
                    _options.UseSsl,
                    token );
            }

            // Note: Only needed if the SMTP server requires authentication.
            if( _options.RequiresAuthentication )
            {
                await client.AuthenticateAsync( _options.User, _options.Password, token );
            }

            await client.SendAsync( message, token );
            await client.DisconnectAsync( true, token );
        }
        catch( Exception ex )
        {
            response.ErrorMessages.Add( ex.Message );
        }

        return response;
    }
}
