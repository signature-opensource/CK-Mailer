using CK.AppIdentity;
using CK.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Mailer;

public class EmailSenderFeatureDriver : ApplicationIdentityFeatureDriver
{
    readonly IEnumerable<EmailSenderFactory> _parties;

    public EmailSenderFeatureDriver( ApplicationIdentityService s, IEnumerable<EmailSenderFactory> parties )
        : base( s, isAllowedByDefault: true )
    {
        _parties = parties;
    }

    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        if( !ApplicationIdentityService.LocalConfiguration.Configuration.ShouldApplyConfiguration( "EmailSender", optOut: false, out var config ) )
        {
            return Task.FromResult( false );
        }

        var success = true;
        var emailSenders = new List<IEmailSender>();

        foreach( var partyConfig in config!.GetChildren() )
        {
            var factory = _parties.FirstOrDefault( party => party.SenderName == partyConfig.Key );
            if( factory is null )
            {
                success = false;
                context.Monitor.Error( $"Could not find {nameof( EmailSenderFactory )} implementation for '{partyConfig.Key}'." );
                continue;
            }

            // Check if party have many senders.
            var children = partyConfig.GetChildren();
            if( children.Count > 0 && children[0].Key is "0" )
            {
                foreach( var child in children )
                {
                    if( factory.TryCreateEmailSender( child, out var emailSender ) )
                    {
                        emailSenders.Add( emailSender );
                    }
                    else
                    {
                        success = false;
                        context.Monitor.Error( $"Could not create email sender for '{partyConfig.Key}[{child.Key}]'." );
                    }
                }
            }
            else if( factory.TryCreateEmailSender( partyConfig, out var emailSender ) )
            {
                emailSenders.Add( emailSender );
            }
            else
            {
                success = false;
                context.Monitor.Error( $"Could not create email sender for '{partyConfig.Key}' from '{factory.GetType().Name}' factory." );
            }
        }
        ApplicationIdentityService.AddFeature( new EmailSenderFeature( emailSenders ) );

        return Task.FromResult( success );
    }

    protected override Task<bool> SetupDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        return SetupAsync( context );
    }

    protected override Task TeardownAsync( FeatureLifetimeContext context )
    {
        return Task.CompletedTask;
    }

    protected override Task TeardownDynamicRemoteAsync( FeatureLifetimeContext context, IOwnedParty party )
    {
        return TeardownAsync( context );
    }
}
