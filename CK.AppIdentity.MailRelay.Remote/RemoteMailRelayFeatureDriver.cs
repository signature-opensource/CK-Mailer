using CK.AppIdentity.Cris;
using CK.Core;
using CK.Mailer;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CK.AppIdentity.MailRelay;

/// <summary>
/// The Remote side of the opt-in "MailRelay" asymmetric feature: each remote that declares "MailRelayRemote" is a relay
/// for its local party (one that doesn't declare "MailRelay"): a sender of the local party (see <see cref="MailSenders"/>),
/// named after the remote (<c>"$CorpRelay"</c>). A local party can have several relays; a remote added later adds a
/// sender, a destroyed one removes it.
/// <code>
/// "MailSender": { "Default": "$CorpRelay" },  // Required when the party has several senders.
/// "Parties": [
///   { "PartyName": "CorpRelay", "MailRelayRemote": { "Sender": "Support" } },  // The relay's "Support" sender.
///   { "PartyName": "PartnerRelay", "MailRelayRemote": true }                   // The relay chooses.
/// ]
/// </code>
/// In a local party that declares "MailRelay" (it is a relay), the remotes that declare "MailRelayRemote" are the
/// parties that may send their mails through it (CK.AppIdentity.MailRelay.Local): they are not senders.
/// </summary>
[PartyFeature( "MailRelay", TypeActivation.Optional, Side = FeatureSide.Remote )]
public sealed class RemoteMailRelayFeatureDriver : ApplicationIdentityFeatureDriver
{
    readonly PocoDirectory _pocos;
    readonly ConcurrentDictionary<IRemoteParty, RelayEmailSender> _relays;

    /// <summary>
    /// Initializes a new <see cref="RemoteMailRelayFeatureDriver"/>.
    /// </summary>
    /// <param name="s">The application identity service.</param>
    /// <param name="cris">The Cris channel feature driver: the mails are sent on the channel to the relay.</param>
    /// <param name="mailSender">The "MailSender" feature driver: the senders of the local parties are set up before.</param>
    /// <param name="pocos">The poco directory.</param>
    public RemoteMailRelayFeatureDriver( ApplicationIdentityService s,
                                         CrisChannelFeatureDriver cris,
                                         MailSenderFeatureDriver mailSender,
                                         PocoDirectory pocos )
        : base( s )
    {
        _pocos = pocos;
        _relays = new ConcurrentDictionary<IRemoteParty, RelayEmailSender>();
    }

    /// <inheritdoc />
    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        bool success = true;
        foreach( var local in context.GetAllLocals() )
        {
            success &= Plug( context.Monitor, local );
        }
        return Task.FromResult( success );
    }

    /// <inheritdoc />
    protected override Task<bool> SetupDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        if( party is ILocalParty local ) return Task.FromResult( Plug( context.Monitor, local ) );
        var remote = (IRemoteParty)party;
        if( IsAllowedFeature( remote.Owner ) || !IsAllowedFeature( remote ) ) return Task.FromResult( true );
        return Task.FromResult( AddRelay( context.Monitor, remote ) );
    }

    /// <inheritdoc />
    protected override Task TeardownDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        if( party is IRemoteParty remote && _relays.TryRemove( remote, out var sender ) )
        {
            remote.Owner.GetFeature<MailSenders>()?.Remove( remote.PartyName, sender );
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task TeardownAsync( FeatureLifetimeContext context ) => Task.CompletedTask;

    bool Plug( IActivityMonitor monitor, ILocalParty local )
    {
        // A relay: its "MailRelayRemote" remotes are its callers (CK.AppIdentity.MailRelay.Local).
        if( IsAllowedFeature( local ) ) return true;
        bool success = true;
        bool any = false;
        foreach( var r in local.Remotes )
        {
            if( !IsAllowedFeature( r ) || _relays.ContainsKey( r ) ) continue;
            success &= AddRelay( monitor, r );
            any = true;
        }
        if( any && local.GetFeature<MailSenders>() is MailSenders senders )
        {
            success &= senders.CheckDefault( monitor );
        }
        return success;
    }

    bool AddRelay( IActivityMonitor monitor, IRemoteParty remote )
    {
        var senders = remote.Owner.GetFeature<MailSenders>();
        if( senders == null )
        {
            monitor.Error( $"'{remote}' declares \"{GetFeatureName( remote )}\" but '{remote.Owner}' has no \"MailSender\" feature (it is off): the relay cannot be one of its senders." );
            return false;
        }
        var senderName = remote.Configuration.TryGetFeatureSection( GetFeatureName( remote ), "Sender" )?.Value;
        if( senderName != null && senderName.Length == 0 ) senderName = null;
        var sender = new RelayEmailSender( remote, _pocos, senderName );
        if( !senders.TryAdd( monitor, remote.PartyName, sender ) ) return false;
        _relays[remote] = sender;
        return true;
    }
}
