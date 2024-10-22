using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.Mailer.Tests;

public class FakeEmailSenderFactory : EmailSenderFactory
{
    public override bool TryCreateEmailSender( IActivityMonitor monitor,
                                               ImmutableConfigurationSection configuration,
                                               [NotNullWhen( true )] out IEmailSender? emailSender )
    {
        emailSender = null;

        if( configuration.Key != SenderName ) return false;
        if( string.IsNullOrEmpty( configuration["Endpoint"] ) ) return false;
        if( string.IsNullOrEmpty( configuration["Token"] ) ) return false;

        emailSender = new FakeEmailSender( configuration["Endpoint"]!, configuration["Token"]! );
        return true;
    }
}
