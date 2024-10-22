using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.Mailer;

[IsMultiple]
public interface IEmailSenderFactory : ISingletonAutoService
{
    /// <summary>
    /// The name of the feature supported by the factory.
    /// </summary>
    string SenderName { get; }

    /// <summary>
    /// Try to create a sender with the passed configuration.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <param name="configuration">The configuration used to create an email sender.</param>
    /// <param name="emailSender">The email sender that will be created.</param>
    /// <returns><c>true</c> if the method can parse the configuration and create an email sender, otherwise <c>false</c>.</returns>
    bool TryCreateEmailSender( IActivityMonitor monitor, ImmutableConfigurationSection configuration, [NotNullWhen( true )] out IEmailSender? emailSender );
}
