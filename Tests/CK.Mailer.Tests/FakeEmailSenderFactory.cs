using CK.Core;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CK.Mailer.Tests;

public class FakeEmailSenderFactory : EmailSenderFactory
{
    public override bool TryCreateEmailSender( IActivityMonitor monitor, ImmutableConfigurationSection configuration, [NotNullWhen( true )] out IEmailSender? emailSender )
    {
        emailSender = null;

        if( configuration.Key != SenderName ) return false;
        if( !configuration.HasChildren ) return false;

        var properties = configuration.GetChildren();

        var endpoint = properties.SingleOrDefault( p => p.Key is "Endpoint" );
        if( endpoint is null || string.IsNullOrEmpty( endpoint.Value ) ) return false;

        var token = properties.SingleOrDefault( p => p.Key is "Token" );
        if( token is null || string.IsNullOrEmpty( token.Value ) ) return false;

        emailSender = new FakeEmailSender( endpoint.Value, token.Value );
        return true;
    }
}
