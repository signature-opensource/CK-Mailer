using CK.Core;
using CK.Mailer;
using System.Threading.Tasks;

namespace CK.AppIdentity.Mailer;

/// <summary>
/// The Local side of the opt-in "MailSender" feature: this application sends the mails of its remotes. A local party that
/// allows "MailSender" (in its "Local") is a MailSender; its remotes that allow "MailSenderRemote" may send their mails
/// through it (the <see cref="MailSenderHandler"/> refuses the others).
/// <para>
/// The mails are sent with the <see cref="IDefaultEmailSender"/> of the MailSender: its "EmailSender" configuration
/// (CK.Mailer), its own <c>From</c> address. A MailSender without "EmailSender" configuration is an error.
/// </para>
/// <para>
/// An application has one role: it references this package (it is a MailSender) or CK.AppIdentity.Mailer.Remote (it is a
/// remote of a MailSender). An application that sends its own mails needs neither: its "EmailSender" configuration is enough.
/// </para>
/// </summary>
public sealed class LocalMailSenderFeatureDriver : ApplicationIdentityFeatureDriver
{
    /// <summary>
    /// The feature name: "MailSender".
    /// </summary>
    public const string MailSenderFeatureName = "MailSender";

    /// <summary>
    /// Initializes a new <see cref="LocalMailSenderFeatureDriver"/>.
    /// </summary>
    /// <param name="s">The application identity service.</param>
    /// <param name="emailSender">The email sender feature driver: the senders must be set up before this driver.</param>
    public LocalMailSenderFeatureDriver( ApplicationIdentityService s, EmailSenderFeatureDriver emailSender )
        : base( s, MailSenderFeatureName, isAllowedByDefault: false )
    {
    }

    /// <summary>
    /// Gets whether the local party that owns the <paramref name="caller"/> is a MailSender and the caller one of its remotes
    /// that may send mails through it.
    /// </summary>
    /// <param name="caller">The remote party.</param>
    /// <returns>True if the caller may send mails through its local party.</returns>
    public bool IsAllowedCaller( IRemoteParty caller ) => IsAllowedFeature( caller.Owner ) && IsAllowedFeature( caller );

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
        if( IsAllowedFeature( local ) && local.GetFeature<IDefaultEmailSender>() == null )
        {
            context.Monitor.Error( $"'{local}' allows the '{MailSenderFeatureName}' feature but has no \"EmailSender\" configuration (in its \"Local\"): it cannot send mails." );
            return false;
        }
        return true;
    }
}
