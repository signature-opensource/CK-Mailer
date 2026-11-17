using CK.AppIdentity;
using CK.Core;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK.Mailer.MailKit;

/// <summary>
/// The "MailKit" option of the "MailSender" feature: the senders of a party that use MailKit (an SMTP server, or a pickup
/// directory). Its section holds the named senders, their settings are MailKit's:
/// <code>
/// "MailSender": {
///   "MailKit": {
///     "NoReply": {
///       "Host": "smtp.acme.com",          // Required when SendEmail is true (the default).
///       "Port": 587,                       // The default.
///       "RequiresAuthentication": true,    // The default: User and Password are required.
///       "User": "...", "Password": "...",
///       "SocketOptions": "StartTls",       // A MailKit SecureSocketOptions (or "UseSsl": true, the default).
///       "From": "noreply@acme.com",
///       "SendEmail": true,                 // False: no mail is sent (with UsePickupDirectory, the mails are written there).
///       "UsePickupDirectory": false, "PickupDirectory": "Mails"
///     }
///   }
/// }
/// </code>
/// An invalid configuration throws.
/// </summary>
[PartyFeatureOption( "MailKit", TypeActivation.Optional )]
public sealed class MailKitOption : MailSenderOption
{
    static readonly string[] _keys = ["SendEmail", "UsePickupDirectory", "PickupDirectory", "Host", "Port", "RequiresAuthentication",
                                      "User", "Password", "SocketOptions", "UseSsl", "From"];

    readonly IReadOnlyDictionary<string, IEmailSender> _senders;
    readonly IReadOnlyDictionary<string, MailKitSenderOptions> _options;

    /// <summary>
    /// Initializes a new <see cref="MailKitOption"/> from its configuration.
    /// </summary>
    /// <param name="configuration">The "MailKit" section: the named senders.</param>
    public MailKitOption( IConfiguration? configuration )
    {
        var options = new Dictionary<string, MailKitSenderOptions>( StringComparer.OrdinalIgnoreCase );
        _senders = ReadNamedSenders( configuration, "MailKit", ( name, c ) =>
        {
            var o = Read( c );
            options.Add( name, o );
            return new MailKitSender( o );
        } );
        _options = options;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, IEmailSender> Senders => _senders;

    /// <summary>
    /// Gets the options of each sender, by name.
    /// </summary>
    public IReadOnlyDictionary<string, MailKitSenderOptions> SenderOptions => _options;

    /// <summary>
    /// Reads the options of a MailKit sender. Throws on an invalid configuration.
    /// </summary>
    /// <param name="config">The sender's section.</param>
    /// <returns>The options.</returns>
    public static MailKitSenderOptions Read( IConfiguration config )
    {
        foreach( var c in config.GetChildren() )
        {
            if( !_keys.Contains( c.Key, StringComparer.OrdinalIgnoreCase ) )
            {
                Throw.CKException( $"Unknown \"{c.Key}\": the keys are {_keys.Concatenate()}." );
            }
        }
        var options = new MailKitSenderOptions();
        if( ReadBool( config, "UsePickupDirectory" ) is bool useDir && (options.UsePickupDirectory = useDir) )
        {
            options.PickupDirectory = config["PickupDirectory"];
            if( string.IsNullOrEmpty( options.PickupDirectory ) ) Throw.CKException( "\"PickupDirectory\" is required when \"UsePickupDirectory\" is true." );
        }
        if( config["From"] is string from )
        {
            if( !InternetAddress.TryParse( from, out var fromAddress ) ) Throw.CKException( $"Invalid \"From\": '{from}' is not a valid mailbox address." );
            options.From = fromAddress;
        }
        if( ReadBool( config, "SendEmail" ) is bool sendEmail ) options.SendEmail = sendEmail;
        if( !options.SendEmail ) return options;

        options.Host = config["Host"];
        if( string.IsNullOrEmpty( options.Host ) ) Throw.CKException( "\"Host\" is required (or \"SendEmail\": false)." );
        if( config["Port"] is string port )
        {
            if( !int.TryParse( port, out var p ) || p <= 0 ) Throw.CKException( $"Invalid \"Port\": '{port}'." );
            options.Port = p;
        }
        if( ReadBool( config, "RequiresAuthentication" ) is bool auth ) options.RequiresAuthentication = auth;
        if( options.RequiresAuthentication )
        {
            options.User = config["User"];
            options.Password = config["Password"];
            if( string.IsNullOrEmpty( options.User ) || string.IsNullOrEmpty( options.Password ) )
            {
                Throw.CKException( "\"User\" and \"Password\" are required when \"RequiresAuthentication\" is true (the default)." );
            }
        }
        if( config["SocketOptions"] is string socket )
        {
            if( !Enum.TryParse<SecureSocketOptions>( socket, ignoreCase: true, out var s ) )
            {
                Throw.CKException( $"Invalid \"SocketOptions\": '{socket}' (the values are {Enum.GetNames<SecureSocketOptions>().Concatenate()})." );
            }
            options.SocketOptions = s;
        }
        else if( ReadBool( config, "UseSsl" ) is bool useSsl )
        {
            options.UseSsl = useSsl;
        }
        return options;
    }

    static bool? ReadBool( IConfiguration config, string key )
    {
        var v = config[key];
        if( v == null ) return null;
        if( !bool.TryParse( v, out var b ) ) Throw.CKException( $"Invalid \"{key}\": '{v}' (true or false is expected)." );
        return b;
    }
}
