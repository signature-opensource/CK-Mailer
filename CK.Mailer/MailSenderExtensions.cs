using CK.AppIdentity;
using CK.Core;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

/// <summary>
/// Extends <see cref="ILocalParty"/> with its senders.
/// </summary>
public static class MailSenderExtensions
{
    /// <summary>
    /// Gets a sender of a local party (see <see cref="MailSenders.GetSender(string?)"/>): the default one when
    /// <paramref name="senderName"/> is null. It is resolved when a mail is sent: a party without the "MailSender"
    /// feature, or without this sender, fails the sending (the <see cref="SendResponse.ErrorMessages"/> say why).
    /// </summary>
    /// <param name="party">The local party.</param>
    /// <param name="senderName">The sender name, null for the default one.</param>
    /// <returns>The sender.</returns>
    public static IEmailSender GetMailSender( this ILocalParty party, string? senderName = null )
    {
        Throw.CheckNotNullArgument( party );
        return party.GetFeature<MailSenders>()?.GetSender( senderName ) ?? new NoSender( party, senderName );
    }

    // The party may get its feature later (it is resolved again on each sending).
    sealed class NoSender : IEmailSender
    {
        readonly ILocalParty _party;
        readonly string? _name;

        public NoSender( ILocalParty party, string? name )
        {
            _party = party;
            _name = name;
        }

        public Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
        {
            if( _party.GetFeature<MailSenders>() is MailSenders s ) return s.GetSender( _name ).SendAsync( monitor, email, token );
            var error = $"'{_party}' has no mail sender: the \"{MailSenderFeatureDriver.MailSenderFeatureName}\" feature is off.";
            monitor.Error( error );
            var r = new SendResponse();
            r.ErrorMessages.Add( error );
            return Task.FromResult( r );
        }
    }
}
