using CK.AppIdentity.Cris;
using CK.Core;
using System.Linq;
using System.Threading.Tasks;

namespace CK.AppIdentity.Mailer;

/// <summary>
/// The Remote side of the opt-in "MailSender" feature: this application sends its mails through a remote party. A local
/// party that allows "MailSenderRemote" (in its "Local") uses <b>the</b> remote that allows "MailSender": it gets a
/// <see cref="RemoteMailSender"/> feature. None, or more than one, is a configuration error. A remote added later
/// (<see cref="ILocalParty.AddRemoteAsync"/>) cannot be the MailSender.
/// <para>
/// The mails are sent by the "MailSender" sender of the "EmailSender" configuration of the local party (see
/// <see cref="MailSenderEmailSenderFactory"/>).
/// </para>
/// <para>
/// An application has one role: it references this package (it is a remote of a MailSender) or CK.AppIdentity.Mailer (it
/// is a MailSender).
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
        : base( s, "MailSender", isAllowedByDefault: false )
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
        if( IsAllowedFeature( party ) && IsAllowedFeature( party.Owner ) )
        {
            context.Monitor.Error( $"Remote '{party}' cannot be the MailSender of '{party.Owner}': the MailSender is defined with its local party, not by a remote added later." );
            return Task.FromResult( false );
        }
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
        if( !IsAllowedFeature( local ) ) return true;
        var senders = local.Remotes.Where( IsAllowedFeature ).ToList();
        if( senders.Count != 1 )
        {
            context.Monitor.Error( senders.Count == 0
                                    ? $"'{local}' allows the 'MailSenderRemote' feature but none of its remotes allows 'MailSender'."
                                    : $"'{local}' allows the 'MailSenderRemote' feature but {senders.Count} of its remotes allow 'MailSender' ({senders.Select( r => r.FullName.Path ).Concatenate()}): there must be only one." );
            return false;
        }
        local.AddFeature( new RemoteMailSender( senders[0] ) );
        return true;
    }
}
