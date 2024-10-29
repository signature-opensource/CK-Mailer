using CK.AppIdentity;
using CK.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Mailer;

public class EmailSenderFeatureDriver : ApplicationIdentityFeatureDriver
{
    readonly IEnumerable<IEmailSenderFactory> _parties;

    public EmailSenderFeatureDriver( ApplicationIdentityService s, IEnumerable<IEmailSenderFactory> parties )
        : base( s, isAllowedByDefault: true )
    {
        _parties = parties;
    }

    protected override Task<bool> SetupAsync( FeatureLifetimeContext context )
    {
        var success = true;
        foreach( var remote in context.GetAllRemotes().Cast<IParty>().Concat( context.GetAllLocals() ).Where( IsAllowedFeature ) )
        {
            success &= PlugFeature( context.Monitor, remote );
        }
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
        return Task.CompletedTask;
    }

    bool PlugFeature( IActivityMonitor monitor, IParty party )
    {
        var section = party switch
        {
            ILocalParty local => local.LocalConfiguration.Configuration.TryGetSection( "EmailSender" ),
            _ => party.Configuration.Configuration.TryGetSection( "EmailSender" )
        };
        if( section is null )
        {
            return true;
        }

        var success = true;
        var emailSenders = new List<IEmailSender>();

        foreach( var partyConfig in section.GetChildren() )
        {
            var factory = _parties.FirstOrDefault( party => party.SenderName == partyConfig.Key );
            if( factory is null )
            {
                monitor.Warn( $"Could not find {nameof( EmailSenderFactory )} implementation for '{partyConfig.Key}'." );
                continue;
            }

            // Check if party have many senders. Here, an array in the configuration.
            if( partyConfig.HasChildren && partyConfig.GetChildren()[0].Key is "0" )
            {
                foreach( var child in partyConfig.GetChildren() )
                {
                    if( factory.TryCreateEmailSender( monitor, child, out var emailSender ) )
                    {
                        emailSenders.Add( emailSender );
                    }
                    else
                    {
                        success = false;
                        monitor.Error( $"Could not create email sender for '{partyConfig.Key}[{child.Key}]'." );
                    }
                }
            }
            else if( factory.TryCreateEmailSender( monitor, partyConfig, out var emailSender ) )
            {
                emailSenders.Add( emailSender );
            }
            else
            {
                success = false;
                monitor.Error( $"Could not create email sender for '{partyConfig.Key}' from '{factory.GetType().Name}' factory." );
            }
        }

        ApplicationIdentityService.AddFeature(
            party == ApplicationIdentityService
                ? new InternalDefaultEmailSender( emailSenders )
                : new EmailSenderFeature( emailSenders ) );

        return success;
    }
}
