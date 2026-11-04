using CK.Core;
using CK.Mailer;
using System.Diagnostics.CodeAnalysis;

namespace CK.AppIdentity.MailSender;

/// <summary>
/// Creates the "MailSender" email sender: the key of the "EmailSender" configuration (CK.Mailer) of a local party that sends
/// its mails through its MailSender (see <see cref="RemoteMailSenderFeatureDriver"/>):
/// <code>
/// "Local": { "AllowFeatures": "MailSenderRemote", "EmailSender": { "MailSender": true } }
/// </code>
/// The sender sends the mails of the application (its root party) to the MailSender of the root party.
/// </summary>
public sealed class MailSenderEmailSenderFactory : EmailSenderFactory
{
    readonly ApplicationIdentityService _identity;
    readonly PocoDirectory _pocos;

    /// <summary>
    /// Initializes a new <see cref="MailSenderEmailSenderFactory"/>.
    /// </summary>
    /// <param name="identity">The application identity service.</param>
    /// <param name="pocos">The poco directory.</param>
    public MailSenderEmailSenderFactory( ApplicationIdentityService identity, PocoDirectory pocos )
    {
        _identity = identity;
        _pocos = pocos;
    }

    /// <inheritdoc />
    public override bool TryCreateEmailSender( IActivityMonitor monitor,
                                               ImmutableConfigurationSection configuration,
                                               [NotNullWhen( true )] out IEmailSender? emailSender )
    {
        if( !bool.TryParse( configuration.Value, out var enabled ) || !enabled )
        {
            monitor.Error( $"The \"MailSender\" email sender configuration must be true (found '{configuration.Value}')." );
            emailSender = null;
            return false;
        }
        emailSender = new RemoteEmailSender( _identity, _pocos );
        return true;
    }
}
