using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.Mailer;

[IsMultiple]
public interface IEmailSenderFactory : ISingletonAutoService
{
    string SenderName { get; }

    bool TryCreateEmailSender( ImmutableConfigurationSection configuration, [NotNullWhen( true )] out IEmailSender? emailSender );
}
