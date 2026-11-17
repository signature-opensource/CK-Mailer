using CK.Core;
using CK.Cris;
using CK.IO.MailRelay;
using CK.Mailer;
using System.IO;
using System.Threading.Tasks;

namespace CK.AppIdentity.MailRelay;

/// <summary>
/// Handles the <see cref="ISendMailCommand"/> of a remote: the mail is sent with a sender of the local party that
/// receives it (a "MailRelay"), with the <c>From</c> address of this sender: the <see cref="ISendMailCommand.Sender"/>,
/// else the relay's configured one (<see cref="LocalMailRelayFeatureDriver.GetConfiguredSender(ILocalParty)"/>), else its
/// default sender. A caller that is not allowed (see <see cref="LocalMailRelayFeatureDriver.IsAllowedCaller(IRemoteParty)"/>),
/// an unknown sender or a relay sender is refused: the result carries the error.
/// </summary>
public sealed class MailRelayHandler : ISingletonAutoService
{
    readonly LocalMailRelayFeatureDriver _driver;

    /// <summary>
    /// Initializes a new <see cref="MailRelayHandler"/>.
    /// </summary>
    /// <param name="driver">The "MailRelay" feature driver.</param>
    public MailRelayHandler( LocalMailRelayFeatureDriver driver )
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
                            ? $"'{caller}' doesn't declare the 'MailRelayRemote' feature: '{caller.Owner}' doesn't send its mails."
                            : $"'{caller.Owner}' is not a MailRelay: it doesn't declare the 'MailRelay' feature.";
            monitor.Warn( error );
            result.ErrorMessages.Add( error );
            return result;
        }
        var senders = caller.Owner.GetFeature<MailSenders>();
        var name = command.Sender ?? _driver.GetConfiguredSender( caller.Owner );
        string? refused = senders == null
                            ? $"'{caller.Owner}' has no sender: its \"MailSender\" feature is off."
                            : name == null
                                ? null
                                : MailSenders.IsRelayName( name )
                                    ? $"'{caller.Owner}' doesn't relay a mail to another relay ('{name}')."
                                    : senders.Find( name ) == null
                                        ? $"'{caller.Owner}' has no sender '{name}' (its senders are '{senders.Names.Concatenate( "', '" )}')."
                                        : null;
        if( refused != null )
        {
            monitor.Error( refused );
            result.ErrorMessages.Add( refused );
            return result;
        }
        Throw.DebugAssert( senders != null );
        var sender = senders.GetSender( name );
        using( monitor.OpenInfo( $"Sending the mail '{command.Subject}' of '{caller}' with the sender '{name ?? senders.DefaultName ?? "(default)"}'." ) )
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
