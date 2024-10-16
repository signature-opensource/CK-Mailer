using CK.Core;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CK.Mailer.MailKit;

public class SmtpEmailSenderFactory : EmailSenderFactory
{
    public override bool TryCreateEmailSender( ImmutableConfigurationSection configuration, [NotNullWhen( true )] out IEmailSender? emailSender )
    {
        emailSender = null;
        var i = configuration.GetChildren();

        var host = i.SingleOrDefault( c => c.Key is "Host" );
        if( host is null || string.IsNullOrEmpty( host.Value ) ) return false;

        var portStr = i.SingleOrDefault( c => c.Key is "Port" );
        if( portStr is null || !int.TryParse( portStr.Value, out var port ) ) return false;

        // TODO: Read the rest of the configuration...

        var options = new MailKitSenderOptions
        {
            Server = host.Value
        };
        emailSender = new MailKitSender( options );
        return true;
    }
}
