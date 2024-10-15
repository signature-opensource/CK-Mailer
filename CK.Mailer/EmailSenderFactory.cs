using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.Mailer;

public abstract class EmailSenderFactory
{
    readonly string _senderName;

    protected EmailSenderFactory()
    {
        Throw.DebugAssert( "EmailSenderFactory".Length == 18 );
        var name = GetType().Name;
        if( name.EndsWith( "EmailSenderFactory_CK" ) ) name = name.Substring( 0, name.Length - 21 );
        else if( name.EndsWith( "EmailSenderFactory" ) ) name = name.Substring( 0, name.Length - 18 );
        else
        {
            Throw.InvalidOperationException( $"Invalid type name '{name}': a feature driver type name MUST be suffixed with 'EmailSenderFactory'." );
        }
        _senderName = name;
    }

    public string SenderName => _senderName;

    public abstract bool TryCreateEmailSender( ImmutableConfigurationSection configuration, [NotNullWhen( true )] out IEmailSender? emailSender );
}
