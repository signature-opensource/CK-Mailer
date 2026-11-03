using CK.Core;
using CK.Cris;
using CK.IO.Mailer;
using CK.Mailer;
using System.IO;
using System.Threading.Tasks;

namespace CK.AppIdentity.Mailer;

/// <summary>
/// Handles the <see cref="ISendMailCommand"/> of a remote: the mail is sent with the <see cref="IDefaultEmailSender"/> of
/// the local party that receives it, with its own <c>From</c> address. A caller that is not allowed (see
/// <see cref="LocalMailSenderFeatureDriver.IsAllowedCaller(IRemoteParty)"/>) is refused: the result carries the error.
/// </summary>
public sealed class MailSenderHandler : ISingletonAutoService
{
    readonly LocalMailSenderFeatureDriver _driver;

    /// <summary>
    /// Initializes a new <see cref="MailSenderHandler"/>.
    /// </summary>
    /// <param name="driver">The "MailSender" feature driver.</param>
    public MailSenderHandler( LocalMailSenderFeatureDriver driver )
    {
        _driver = driver;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <param name="command">The command.</param>
    /// <param name="caller">The remote that sent it.</param>
    /// <param name="pocos">The poco directory.</param>
    /// <returns>The result.</returns>
    [CommandHandler]
    public async Task<ISendMailResult> HandleAsync( IActivityMonitor monitor, ISendMailCommand command, IRemoteParty caller, PocoDirectory pocos )
    {
        var result = pocos.Create<ISendMailResult>();
        if( !_driver.IsAllowedCaller( caller ) )
        {
            var error = _driver.IsAllowedFeature( caller.Owner )
                            ? $"'{caller}' doesn't allow the 'MailSenderRemote' feature: '{caller.Owner}' doesn't send its mails."
                            : $"'{caller.Owner}' is not a MailSender: it doesn't allow the 'MailSender' feature.";
            monitor.Warn( error );
            result.ErrorMessages.Add( error );
            return result;
        }
        var sender = caller.Owner.GetFeature<IDefaultEmailSender>();
        if( sender == null )
        {
            var error = $"'{caller.Owner}' has no \"EmailSender\" configuration.";
            monitor.Error( error );
            result.ErrorMessages.Add( error );
            return result;
        }
        using( monitor.OpenInfo( $"Sending the mail '{command.Subject}' of '{caller}'." ) )
        {
            var response = await sender.SendAsync( monitor, ToSimpleEmail( command ) ).ConfigureAwait( false );
            result.MessageId = response.MessageId;
            foreach( var e in response.ErrorMessages ) result.ErrorMessages.Add( e );
            monitor.CloseGroup( response.Successful ? "Sent." : "Failed." );
        }
        return result;
    }

    // The From is never taken from the command (it has none): the sender uses its configured address.
    static SimpleEmail ToSimpleEmail( ISendMailCommand command )
    {
        var email = new SimpleEmail
        {
            Subject = command.Subject,
            HtmlBody = command.HtmlBody,
            PlaintextAlternativeBody = command.PlaintextBody,
            Priority = (Priority)command.Priority
        };
        foreach( var a in command.To ) email.ToAddresses.Add( new Address( a.Email, a.Name ) );
        foreach( var a in command.Cc ) email.CcAddresses.Add( new Address( a.Email, a.Name ) );
        foreach( var a in command.Bcc ) email.BccAddresses.Add( new Address( a.Email, a.Name ) );
        foreach( var a in command.ReplyTo ) email.ReplyToAddresses.Add( new Address( a.Email, a.Name ) );
        foreach( var (k, v) in command.Headers ) email.Headers.Add( k, v );
        foreach( var a in command.Attachments )
        {
            email.Attachments.Add( new Attachment
            {
                Filename = a.FileName,
                ContentType = a.ContentType,
                ContentId = a.ContentId,
                IsInline = a.IsInline,
                Data = new MemoryStream( a.Data, writable: false )
            } );
        }
        return email;
    }
}
