using CK.AppIdentity;
using CK.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace CK.Mailer;

/// <summary>
/// Base class of the options of the "MailSender" feature that define senders: one option per sender kind ("MailKit"),
/// its section holds the named senders of this kind (<c>"MailKit": { "NoReply": {...}, "Support": {...} }</c>). A
/// concrete option is decorated with its <see cref="PartyFeatureOptionAttribute"/> and reads its section in its
/// constructor (see <see cref="ReadNamedSenders"/>).
/// </summary>
public abstract class MailSenderOption : IPartyComponentOption<MailSenderFeatureDriver>
{
    /// <summary>
    /// Gets the senders defined by this option, by name.
    /// </summary>
    public abstract IReadOnlyDictionary<string, IEmailSender> Senders { get; }

    /// <summary>
    /// Reads the named senders of a sender kind: each child of the section is a sender, its key is the sender name (see
    /// <see cref="MailSenders.IsValidSenderName(string?)"/>). Throws on an invalid configuration.
    /// </summary>
    /// <param name="configuration">The option's section (null when enabled by <c>true</c>: an error).</param>
    /// <param name="kind">The option name ("MailKit").</param>
    /// <param name="create">Creates a sender from its name and its section (throws on an invalid configuration).</param>
    /// <returns>The senders.</returns>
    protected static IReadOnlyDictionary<string, IEmailSender> ReadNamedSenders( IConfiguration? configuration,
                                                                                 string kind,
                                                                                 Func<string, IConfiguration, IEmailSender> create )
    {
        var senders = new Dictionary<string, IEmailSender>( StringComparer.OrdinalIgnoreCase );
        if( configuration != null )
        {
            foreach( var c in configuration.GetChildren() )
            {
                if( !MailSenders.IsValidSenderName( c.Key ) )
                {
                    Throw.CKException( $"Invalid sender name '{c.Key}': a sender name is an identifier (letters, digits, '_', '-' and '.')." );
                }
                try
                {
                    senders.Add( c.Key, create( c.Key, c ) );
                }
                catch( Exception ex )
                {
                    Throw.CKException( $"Invalid sender '{c.Key}': {ex.Message}", ex );
                }
            }
        }
        if( senders.Count == 0 )
        {
            Throw.CKException( $"\"{kind}\" must be a section of named senders: \"{kind}\": {{ \"NoReply\": {{ ... }} }}." );
        }
        return senders;
    }
}
