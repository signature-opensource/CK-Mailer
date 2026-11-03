namespace CK.AppIdentity.Mailer;

/// <summary>
/// The feature of a local party that sends its mails through a remote: its <see cref="MailSender"/>.
/// Added by the <see cref="RemoteMailSenderFeatureDriver"/>.
/// </summary>
public sealed class RemoteMailSender
{
    internal RemoteMailSender( IRemoteParty mailSender )
    {
        MailSender = mailSender;
    }

    /// <summary>
    /// Gets the remote party that sends the mails.
    /// </summary>
    public IRemoteParty MailSender { get; }
}
