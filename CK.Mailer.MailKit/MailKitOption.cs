using CK.AppIdentity;
using CK.Core;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK.Mailer.MailKit;

/// <summary>
/// The "MailKit" option of the "MailSender" feature: the senders of a party that use MailKit (an SMTP server, or a pickup
/// directory). Its configuration is a model (<see cref="MailKitOptionConfiguration"/>): the named senders, their settings
/// are MailKit's:
/// <code>
/// "MailSender": {
///   "MailKit": {
///     "Senders": {
///       "NoReply": {
///         "Host": "smtp.acme.com",          // Required when SendEmail is true (the default).
///         "Port": 587,                       // The default.
///         "RequiresAuthentication": true,    // The default: User and Password are required.
///         "User": "...", "Password": { "$SECRET": "Smtp:Password" },
///         "SocketOptions": "StartTls",       // A MailKit SecureSocketOptions ("Auto" by default).
///         "From": "noreply@acme.com",
///         "SendEmail": true,                 // False: no mail is sent (with UsePickupDirectory, the mails are written there).
///         "UsePickupDirectory": false, "PickupDirectory": "Mails"
///       }
///     }
///   }
/// }
/// </code>
/// </summary>
[PartyFeatureOption( "MailKit", TypeActivation.Optional )]
[Configuration<MailKitOptionConfiguration>]
[ConfigurationSecret( "Party:Senders:*:Password" )]
public sealed class MailKitOption : MailSenderOption
{
    readonly IReadOnlyDictionary<string, IEmailSender> _senders;
    readonly IReadOnlyDictionary<string, MailKitSenderOptions> _options;

    /// <summary>
    /// Initializes a new <see cref="MailKitOption"/> from its configuration.
    /// </summary>
    /// <param name="configuration">The (checked) configuration: the named senders.</param>
    public MailKitOption( MailKitOptionConfiguration configuration )
    {
        _options = configuration.Senders.ToDictionary( kv => kv.Key, kv => kv.Value.CreateOptions(), StringComparer.OrdinalIgnoreCase );
        _senders = _options.ToDictionary( kv => kv.Key, kv => (IEmailSender)new MailKitSender( kv.Value ), StringComparer.OrdinalIgnoreCase );
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, IEmailSender> Senders => _senders;

    /// <summary>
    /// Gets the options of each sender, by name.
    /// </summary>
    public IReadOnlyDictionary<string, MailKitSenderOptions> SenderOptions => _options;
}

/// <summary>
/// The configuration of the "MailKit" option of the "MailSender" feature: its senders by name.
/// </summary>
[ConfigurationModel]
public sealed class MailKitOptionConfiguration : IConfigurationModel
{
    /// <summary>
    /// Gets the senders by name (see <see cref="MailSenders.IsValidSenderName(string?)"/>): at least one, a name is unique
    /// across the sender kinds of a party.
    /// </summary>
    public IReadOnlyDictionary<string, MailKitSenderConfiguration> Senders { get; init; } = new Dictionary<string, MailKitSenderConfiguration>();

    /// <inheritdoc />
    public void Validate( IConfigurationValidationContext context )
    {
        if( Senders.Count == 0 )
        {
            context.Error( $"'{context.Path}:Senders' must define at least one sender: \"Senders\": {{ \"NoReply\": {{ ... }} }}." );
        }
        foreach( var name in Senders.Keys.Where( n => !MailSenders.IsValidSenderName( n ) ) )
        {
            context.Error( $"'{context.Path}:Senders:{name}': invalid sender name (letters, digits, '_', '-' and '.')." );
        }
    }
}

/// <summary>
/// The configuration of a MailKit sender (see <see cref="MailKitSenderOptions"/>).
/// </summary>
[ConfigurationModel]
public sealed class MailKitSenderConfiguration : IConfigurationModel
{
    /// <summary>
    /// Gets whether the mails are sent (false: none is sent; with <see cref="UsePickupDirectory"/>, they are written there).
    /// </summary>
    public bool SendEmail { get; init; } = true;

    /// <summary>
    /// Gets whether the mails are written in the <see cref="PickupDirectory"/>.
    /// </summary>
    public bool UsePickupDirectory { get; init; }

    /// <summary>
    /// Gets the directory where the mails are written (required when <see cref="UsePickupDirectory"/> is true).
    /// </summary>
    public string? PickupDirectory { get; init; }

    /// <summary>
    /// Gets the SMTP server (required when <see cref="SendEmail"/> is true).
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// Gets the port of the SMTP server.
    /// </summary>
    public int Port { get; init; } = 587;

    /// <summary>
    /// Gets whether the SMTP server requires an authentication (<see cref="User"/> and <see cref="Password"/> are required).
    /// </summary>
    public bool RequiresAuthentication { get; init; } = true;

    /// <summary>
    /// Gets the user name at the SMTP server.
    /// </summary>
    public string? User { get; init; }

    /// <summary>
    /// Gets the password at the SMTP server: a secret, its configuration is a handle (<c>{ "$SECRET": "&lt;Name&gt;" }</c>).
    /// </summary>
    [ConfigurationSecret]
    public string? Password { get; init; }

    /// <summary>
    /// Gets how the connection is secured (MailKit's choice from the port by default).
    /// </summary>
    public SecureSocketOptions SocketOptions { get; init; } = SecureSocketOptions.Auto;

    /// <summary>
    /// Gets the mailbox the mails are sent from (when the mail has none).
    /// </summary>
    public string? From { get; init; }

    /// <inheritdoc />
    public void Validate( IConfigurationValidationContext context )
    {
        if( UsePickupDirectory && string.IsNullOrWhiteSpace( PickupDirectory ) )
        {
            context.Error( $"'{context.Path}:PickupDirectory' is required when \"UsePickupDirectory\" is true." );
        }
        if( From != null && !InternetAddress.TryParse( From, out _ ) )
        {
            context.Error( $"'{context.Path}:From' = '{From}' is not a valid mailbox address." );
        }
        if( !SendEmail ) return;
        if( string.IsNullOrWhiteSpace( Host ) )
        {
            context.Error( $"'{context.Path}:Host' is required (or \"SendEmail\": false)." );
        }
        if( Port <= 0 )
        {
            context.Error( $"'{context.Path}:Port' = '{Port}': a positive port is expected." );
        }
        if( RequiresAuthentication && (string.IsNullOrWhiteSpace( User ) || string.IsNullOrWhiteSpace( Password )) )
        {
            context.Error( $"'{context.Path}': \"User\" and \"Password\" are required when \"RequiresAuthentication\" is true (the default)." );
        }
    }

    internal MailKitSenderOptions CreateOptions()
    {
        var o = new MailKitSenderOptions
        {
            UsePickupDirectory = UsePickupDirectory,
            PickupDirectory = UsePickupDirectory ? PickupDirectory : null,
            From = From != null ? InternetAddress.Parse( From ) : null,
            SendEmail = SendEmail,
            SocketOptions = SocketOptions
        };
        if( SendEmail )
        {
            o.Host = Host;
            o.Port = Port;
            o.RequiresAuthentication = RequiresAuthentication;
            if( RequiresAuthentication )
            {
                o.User = User;
                o.Password = Password;
            }
        }
        return o;
    }
}
