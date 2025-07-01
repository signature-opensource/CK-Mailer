using CK.Core;
using MailKit.Security;
using System;
using System.Diagnostics.CodeAnalysis;
using MimeKit;

namespace CK.Mailer.MailKit;

public class SmtpEmailSenderFactory : EmailSenderFactory
{
    public override bool TryCreateEmailSender( IActivityMonitor monitor,
                                               ImmutableConfigurationSection config,
                                               [NotNullWhen( true )] out IEmailSender? emailSender )
    {
        Throw.DebugAssert( config.Key is "Smtp" || int.TryParse( config.Key, out _ ) && config.Path.EndsWith( ":Smtp:" + config.Key ) );

        var success = true;
        var options = new MailKitSenderOptions();

        if( config["UsePickupDirectory"] is not null )
        {
            if( !bool.TryParse( config["UsePickupDirectory"], out var useDir ) )
            {
                monitor.Error( "Invalid 'UsePickupDirectory' configuration in Smtp EmailSender." );
                success = false;
            }
            else if( (options.UsePickupDirectory = useDir)
                && string.IsNullOrEmpty( options.PickupDirectory = config["PickupDirectory"] ) )
            {
                monitor.Error( "Invalid 'PickupDirectory' configuration in Smtp EmailSender." );
                success = false;
            }
        }

        if( config["SendEmail"] is not null )
        {
            if( !bool.TryParse( config["SendEmail"], out var sendEmail ) )
            {
                monitor.Error( "Invalid 'SendEmail' configuration in Smtp EmailSender." );
                emailSender = null;
                // Retrun here because the rest of the configuration depends on SendEmail.
                return false;
            }
            options.SendEmail = sendEmail;
        }

        if( !options.SendEmail )
        {
            emailSender = success ? new MailKitSender( options ) : null;
            return success;
        }

        if( string.IsNullOrEmpty( options.Host = config["Host"] ) )
        {
            monitor.Error( "Invalid or missing 'Host' in Smtp EmailSender configuration." );
            success = false;
        }

        if( config["Port"] is not null )
        {
            if( int.TryParse( config["Port"], out var port ) )
            {
                options.Port = port;
            }
            else
            {
                monitor.Error( "Invalid 'Port' in Smtp EmailSender configuration." );
                success = false;
            }
        }

        if( config["RequiresAuthentication"] is not null )
        {
            if( bool.TryParse( config["RequiresAuthentication"], out var requiredAuth ) )
            {
                options.RequiresAuthentication = requiredAuth;
            }
            else
            {
                monitor.Error( "Invalid 'RequiresAuthentication' in Smtp EmailSender configuration." );
                success = false;
                // Here the configuration is invalid and must igonre the user/password
                // verification, as these properties depends on RequiresAuthentication.
                options.RequiresAuthentication = false;
            }
        }

        if( options.RequiresAuthentication )
        {
            if( string.IsNullOrEmpty( options.User = config["User"] ) )
            {
                monitor.Error( "Invalid or missing 'User' in Smtp EmailSender configuration." );
                success = false;
            }
            if( string.IsNullOrEmpty( options.Password = config["Password"] ) )
            {
                monitor.Error( "Invalid or missing 'Password' in Smtp EmailSender configuration." );
                success = false;
            }
        }

        if( config["SocketOptions"] is not null )
        {
            if( Enum.TryParse<SecureSocketOptions>( config["SocketOptions"], out var socketOptions ) )
            {
                options.SocketOptions = socketOptions;
            }
            else
            {
                monitor.Error( "Invalid 'SocketOptions' in Smtp EmailSender configuration." );
                success = false;
            }
        }
        else if( config["UseSsl"] is not null )
        {
            if( bool.TryParse( config["UseSsl"], out var useSsl ) )
            {
                options.UseSsl = useSsl;
            }
            else
            {
                monitor.Error( "Invalid 'UseSsl' in Smtp EmailSender configuration." );
                success = false;
            }
        }

        if( config["From"] is not null )
        {
            if( !InternetAddress.TryParse( config["From"], out var fromAddress ) )
            {
                monitor.Error( $"Invalid 'From' configuration in Smtp EmailSender. '{fromAddress}' is not a valid mailbox address." );
                emailSender = null;
                // Return here because the rest of the configuration depends on SendEmail.
                return false;
            }
            options.From = fromAddress;
        }

        emailSender = success ? new MailKitSender( options ) : null;
        return success;
    }
}
