using CK.AppIdentity;
using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Mailer;

/// <summary>
/// The "MailSender" feature: how a local party (the root or a tenant domain) sends its mails. Each local party on which
/// it is allowed gets its <see cref="MailSenders"/> feature: its named senders, brought by the options of this feature
/// (the <see cref="MailSenderOption"/>: "MailKit" of CK.Mailer.MailKit) and by the packages that add senders at run
/// time (each remote that declares "MailRelayRemote" is a sender, see CK.AppIdentity.MailRelay.Remote).
/// <code>
/// "MailSender": {
///   "MailKit": {
///     "Senders": {
///       "NoReply": { "Host": "smtp.acme.com", "From": "noreply@acme.com" },
///       "Support": { "Host": "smtp.acme.com", "From": "support@acme.com" }
///     }
///   },
///   "Default": "NoReply"  // Required when there are several senders (a relay is named after its remote: "$CorpRelay").
/// }
/// </code>
/// Enabled by default: a local party without "MailSender" section has no sender (sending a mail fails with an error).
/// A remote has no "MailSender": declaring it on a remote is an error.
/// </summary>
[PartyFeature( MailSenderFeatureName, TypeActivation.ByDefault )]
[Configuration<MailSenderConfiguration>]
public sealed class MailSenderFeatureDriver : ApplicationIdentityFeatureDriver
{
    /// <summary>
    /// The feature name: "MailSender".
    /// </summary>
    public const string MailSenderFeatureName = "MailSender";

    /// <summary>
    /// Initializes a new <see cref="MailSenderFeatureDriver"/>.
    /// </summary>
    /// <param name="s">The application identity service.</param>
    public MailSenderFeatureDriver( ApplicationIdentityService s )
        : base( s )
    {
    }

    /// <inheritdoc />
    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        bool success = true;
        foreach( var local in context.GetAllLocals() )
        {
            success &= Plug( context.Monitor, local );
        }
        foreach( var remote in context.GetAllRemotes() )
        {
            success &= CheckRemote( context.Monitor, remote );
        }
        return Task.FromResult( success );
    }

    /// <inheritdoc />
    protected override Task<bool> SetupDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        return Task.FromResult( party is ILocalParty local
                                    ? Plug( context.Monitor, local )
                                    : CheckRemote( context.Monitor, (IRemoteParty)party ) );
    }

    /// <inheritdoc />
    protected override Task TeardownDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party ) => Task.CompletedTask;

    /// <inheritdoc />
    protected override Task TeardownAsync( FeatureLifetimeContext context ) => Task.CompletedTask;

    bool Plug( IActivityMonitor monitor, ILocalParty local )
    {
        if( !IsAllowedFeature( local ) || local.GetFeature<MailSenders>() != null ) return true;
        bool success = true;
        var senders = new Dictionary<string, IEmailSender>( StringComparer.OrdinalIgnoreCase );
        foreach( var option in GetOptions( local ).OfType<MailSenderOption>() )
        {
            foreach( var (name, sender) in option.Senders )
            {
                if( !senders.TryAdd( name, sender ) )
                {
                    monitor.Error( $"'{local}': the sender '{name}' is defined twice in its \"{MailSenderFeatureName}\" (a sender name is unique across the sender kinds)." );
                    success = false;
                }
            }
        }
        var defaultName = GetModel<MailSenderConfiguration>( local ).Default;
        if( defaultName != null && defaultName.Length == 0 ) defaultName = null;
        var mailSenders = new MailSenders( local, senders, defaultName );
        // A relay (named "$...") is added later by its driver: only a configured sender can be checked here.
        if( defaultName != null && !MailSenders.IsRelayName( defaultName ) && !senders.ContainsKey( defaultName ) )
        {
            monitor.Error( $"'{local}': the \"{MailSenderFeatureName}:Default\" sender '{defaultName}' is not defined (the senders are '{senders.Keys.Concatenate( "', '" )}')." );
            success = false;
        }
        success &= mailSenders.CheckDefault( monitor );
        local.AddFeature( mailSenders );
        return success;
    }

    // Only the remote's own keys: a "Defaults" that declares "MailSender" for the local parties applies to the
    // remotes below it too, it is not an error.
    static bool CheckRemote( IActivityMonitor monitor, IRemoteParty remote )
    {
        if( remote.Configuration.Configuration.TryGetSection( MailSenderFeatureName ) != null )
        {
            monitor.Error( $"'{remote}' declares \"{MailSenderFeatureName}\": a remote has no mail sender (\"{MailSenderFeatureName}\" is a feature of a local party)." );
            return false;
        }
        return true;
    }
}

/// <summary>
/// The configuration of the "MailSender" feature of a local party (its senders are the ones of its options).
/// </summary>
[ConfigurationModel]
public sealed class MailSenderConfiguration
{
    /// <summary>
    /// Gets the name of the default sender: required when there are several senders (a relay is named after its remote:
    /// "$CorpRelay").
    /// </summary>
    public string? Default { get; init; }
}
