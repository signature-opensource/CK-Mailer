using CK.AppIdentity;
using CK.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

/// <summary>
/// Registers the <see cref="IDefaultEmailSender"/>: the default sender of the root party (see
/// <see cref="MailSenders.Default"/>), resolved when a mail is sent.
/// </summary>
/// <remarks>
/// If StObjs are not available, call <see cref="AddDefaultEmailSender"/>.
/// </remarks>
public sealed class DefaultEmailSenderRegistration : IRealObject
{
    void ConfigureServices( StObjContextRoot.ServiceRegister services )
    {
        AddDefaultEmailSender( services.Services );
    }

    /// <summary>
    /// Registers the default email sender service.
    /// </summary>
    /// <param name="services">The service collection to which the default email sender will be added.</param>
    public static void AddDefaultEmailSender( IServiceCollection services ) =>
        services.AddSingleton<IDefaultEmailSender>( s => new RootDefaultSender( s.GetRequiredService<ApplicationIdentityService>() ) );

    sealed class RootDefaultSender : IDefaultEmailSender
    {
        readonly ApplicationIdentityService _root;

        public RootDefaultSender( ApplicationIdentityService root ) => _root = root;

        public Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
        {
            return _root.GetMailSender( null ).SendAsync( monitor, email, token );
        }
    }
}
