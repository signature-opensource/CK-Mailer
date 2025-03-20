using CK.AppIdentity;
using CK.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CK.Mailer
{
    /// <summary>
    /// Temporary. Should not be a real object.
    /// </summary>
    /// <remarks>
    /// If StObjs are not already available,
    /// call <see cref="AddDefaultEmailSender"/>.
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
            services.AddSingleton<IDefaultEmailSender>( s =>
                s.GetRequiredService<ApplicationIdentityService>().GetRequiredFeature<InternalDefaultEmailSender>() );
    }
}
