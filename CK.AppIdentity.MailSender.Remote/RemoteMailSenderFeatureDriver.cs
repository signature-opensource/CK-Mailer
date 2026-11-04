using CK.AppIdentity.Cris;
using CK.Core;
using System.Threading.Tasks;

namespace CK.AppIdentity.MailSender;

/// <summary>
/// The Remote side of the opt-in "MailSender" asymmetric feature: a local party that doesn't allow "MailSender" (in its
/// "Local") sends its mails through <b>the</b> remote that allows "MailSenderRemote", if any: it gets a
/// <see cref="RemoteMailSender"/> feature. More than one is a configuration error; a remote added later can be it. A local
/// party that allows "MailSender" is ignored (CK.AppIdentity.MailSender.Local handles it).
/// <para>
/// The mails are sent by the "MailSender" sender of the "EmailSender" configuration of the local party (see
/// <see cref="MailSenderEmailSenderFactory"/>).
/// </para>
/// <para>
/// This package and CK.AppIdentity.MailSender.Local can be referenced by the same application: the "MailSender" of each
/// local party gives its role.
/// </para>
/// </summary>
public sealed class RemoteMailSenderFeatureDriver : ApplicationIdentityFeatureDriver
{
    /// <summary>
    /// Initializes a new <see cref="RemoteMailSenderFeatureDriver"/>.
    /// </summary>
    /// <param name="s">The application identity service.</param>
    /// <param name="cris">The Cris channel feature driver: the mails are sent on the channel to the MailSender.</param>
    public RemoteMailSenderFeatureDriver( ApplicationIdentityService s, CrisChannelFeatureDriver cris )
        : base( s, "MailSender" )
    {
    }

    /// <inheritdoc />
    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        bool success = true;
        foreach( var local in context.GetAllLocals() )
        {
            success &= Plug( context, local );
        }
        return Task.FromResult( success );
    }

    /// <inheritdoc />
    protected override Task<bool> SetupDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        if( party is ILocalParty local ) return Task.FromResult( Plug( context, local ) );
        var remote = (IRemoteParty)party;
        var owner = remote.Owner;
        if( IsAllowedFeature( owner ) || !IsAllowedFeature( remote ) ) return Task.FromResult( true );
        // A remote added later can be the MailSender of its local party (it is not yet in the Remotes of its local
        // party): the local party can use only one.
        if( owner.GetFeature<RemoteMailSender>() is RemoteMailSender existing )
        {
            context.Monitor.Error( $"'{remote}' allows the 'MailSenderRemote' feature but '{owner}' already uses the MailSender '{existing.MailSender}': "
                                   + $"a local party can use only one MailSender." );
            return Task.FromResult( false );
        }
        owner.AddFeature( new RemoteMailSender( remote ) );
        return Task.FromResult( true );
    }

    /// <inheritdoc />
    protected override Task TeardownDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        if( party is IRemoteParty remote && remote.Owner.GetFeature<RemoteMailSender>()?.MailSender == remote )
        {
            context.Monitor.Error( $"The MailSender '{remote}' has been destroyed: '{remote.Owner}' cannot send mails anymore." );
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task TeardownAsync( FeatureLifetimeContext context ) => Task.CompletedTask;

    bool Plug( FeatureLifetimeContext context, ILocalParty local )
    {
        if( !TryGetFeatureRemote( context.Monitor, local, out var mailSender ) ) return false;
        // The local party is a MailSender, it has no remote MailSender, or it is already plugged (a dynamic tenant domain
        // and its remotes).
        if( mailSender == null || local.GetFeature<RemoteMailSender>() != null ) return true;
        local.AddFeature( new RemoteMailSender( mailSender ) );
        return true;
    }
}
