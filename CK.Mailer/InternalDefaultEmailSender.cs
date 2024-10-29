using CK.AppIdentity;
using CK.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CK.Mailer;

internal sealed class InternalDefaultEmailSender : EmailSenderFeature, IDefaultEmailSender
{
    internal InternalDefaultEmailSender( IEnumerable<IEmailSender> senders ) : base( senders )
    {
    }
}

/// <summary>
/// Temporary. Should not be a real object...
/// </summary>
public sealed class DefautlEmailSenderRegistration : IRealObject
{
    void ConfigureServices( StObjContextRoot.ServiceRegister services )
    {
        services.Services.AddSingleton<IDefaultEmailSender>( s => s.GetRequiredService<ApplicationIdentityService>().GetRequiredFeature<InternalDefaultEmailSender>() );
    }
}
