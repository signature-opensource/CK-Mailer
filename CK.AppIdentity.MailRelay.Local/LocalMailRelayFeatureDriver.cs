using CK.Core;
using CK.Mailer;
using System.Threading.Tasks;

namespace CK.AppIdentity.MailRelay;

/// <summary>
/// The Local side of the opt-in "MailRelay" asymmetric feature: a local party that declares "MailRelay" (in its "Local")
/// sends the mails of its remotes that declare "MailRelayRemote" (the <see cref="MailRelayHandler"/> refuses the others).
/// A local party that doesn't declare "MailRelay" is ignored (CK.AppIdentity.MailRelay.Remote handles it: its
/// "MailRelayRemote" remotes are its relays). There can be several relays in a tenant.
/// <code>
/// "MailRelay": { "Sender": "Relayed" },  // Optional (or "MailRelay": true): the sender used when the caller names none.
/// "MailSender": {
///   "MailKit": { "Senders": { "NoReply": { ... }, "Relayed": { ... } } },
///   "Default": "NoReply"
/// }
/// </code>
/// <para>
/// The mails are sent with a sender of the relay ("MailSender" feature, CK.Mailer): the one that the caller names
/// (<c>"MailRelayRemote": { "Sender": "..." }</c> on its remote), else the relay's <c>"MailRelay": { "Sender" }</c>, else its
/// default sender. A relay has no relay sender (its "MailRelayRemote" remotes are its callers): a mail is never relayed twice.
/// A relay without sender is an error.
/// </para>
/// <para>
/// This package and CK.AppIdentity.MailRelay.Remote can be referenced by the same application: the "MailRelay" of each
/// local party gives its role. An application that sends its own mails needs neither: its "MailSender" is enough.
/// </para>
/// </summary>
[PartyFeature( MailRelayFeatureName, TypeActivation.Optional, Side = FeatureSide.Local )]
[Requires<RemoteMailRelayFeatureDriver>]
public sealed class LocalMailRelayFeatureDriver : ApplicationIdentityFeatureDriver
{
    /// <summary>
    /// The feature name: "MailRelay".
    /// </summary>
    public const string MailRelayFeatureName = "MailRelay";

    /// <summary>
    /// Initializes a new <see cref="LocalMailRelayFeatureDriver"/>.
    /// </summary>
    /// <param name="s">The application identity service.</param>
    /// <param name="mailSender">The "MailSender" feature driver: the senders must be set up before this driver.</param>
    public LocalMailRelayFeatureDriver( ApplicationIdentityService s, MailSenderFeatureDriver mailSender )
        : base( s )
    {
    }

    /// <summary>
    /// Gets whether the local party that owns the <paramref name="caller"/> is a relay and the caller one of its remotes
    /// that may send mails through it.
    /// </summary>
    /// <param name="caller">The remote party.</param>
    /// <returns>True if the caller may send mails through its local party.</returns>
    public bool IsAllowedCaller( IRemoteParty caller ) => IsAllowedFeature( caller.Owner ) && IsAllowedFeature( caller );

    /// <summary>
    /// Gets the configured sender of a relay (<c>"MailRelay": { "Sender": "..." }</c>): the one used when the caller names
    /// none. Null: the relay's default sender.
    /// </summary>
    /// <param name="relay">The local party that is a relay.</param>
    /// <returns>The sender name or null.</returns>
    public string? GetConfiguredSender( ILocalParty relay )
    {
        var name = relay.LocalConfiguration.TryGetFeatureSection( MailRelayFeatureName, "Sender" )?.Value;
        return string.IsNullOrEmpty( name ) ? null : name;
    }

    /// <inheritdoc />
    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        bool success = true;
        foreach( var local in context.GetAllLocals() )
        {
            success &= Check( context, local );
        }
        return Task.FromResult( success );
    }

    /// <inheritdoc />
    protected override Task<bool> SetupDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        return Task.FromResult( party is not ILocalParty local || Check( context, local ) );
    }

    /// <inheritdoc />
    protected override Task TeardownDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party ) => Task.CompletedTask;

    /// <inheritdoc />
    protected override Task TeardownAsync( FeatureLifetimeContext context ) => Task.CompletedTask;

    bool Check( FeatureLifetimeContext context, ILocalParty local )
    {
        if( !IsAllowedFeature( local ) ) return true;
        var senders = local.GetFeature<MailSenders>();
        if( senders == null || senders.Names.Count == 0 )
        {
            context.Monitor.Error( $"'{local}' declares the '{MailRelayFeatureName}' feature but has no sender (its \"MailSender\" defines none): it cannot send mails." );
            return false;
        }
        var configured = GetConfiguredSender( local );
        if( configured != null && senders.Find( configured ) == null )
        {
            context.Monitor.Error( $"'{local}': the \"{MailRelayFeatureName}:Sender\" sender '{configured}' is not defined (the senders are '{senders.Names.Concatenate( "', '" )}')." );
            return false;
        }
        return true;
    }
}
