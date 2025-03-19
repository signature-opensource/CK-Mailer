using CK.Core;
using CK.Mailer.MailKit;
using Shouldly;
using MailKit.Security;
using NUnit.Framework;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

[TestFixture]
public class SmtpEmailSenderFactoryTests
{
    [TestCase("\"\"" )]
    [TestCase( "3712" )]
    public void Invalid_use_pickup_directory( string directory )
    {
        var json = @$"{{""UsePickupDirectory"":{directory}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [Test]
    public void Missing_pickup_directory()
    {
        var json = @"{""UsePickupDirectory"":true}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [TestCase( "null" )]
    [TestCase( "" )]
    public void Invalid_pickup_directory( string directory )
    {
        var json = @$"{{""UsePickupDirectory"":true,""PickupDirectory"":""{directory}""}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [Test]
    public void Only_pickup_directory()
    {
        var json = @"{""UsePickupDirectory"":true,""PickupDirectory"":""Path"",""SendEmail"":false}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }

    [TestCase( "true" )]
    [TestCase( "false" )]
    public void Valid_semd_email( string sendEmail )
    {
        var json = @$"{{""SendEmail"":{sendEmail},""Host"":""Test"",""RequiresAuthentication"":false}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }

    [TestCase( "null" )]
    [TestCase( @"""Test""" )]
    [TestCase( "3712" )]
    public void Invalid_send_email( string sendMesage )
    {
        var json = @$"{{""SendEmail"":{sendMesage}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [Test]
    public void Missing_host_directory()
    {
        var json = @"{""RequiresAuthentication"":false}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [TestCase( "null" )]
    [TestCase( @"""""" )]
    public void Invalid_host_directory( string host )
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":{host}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [TestCase( @"""Test""" )]
    [TestCase( "true" )]
    public void Invalid_port( string port )
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":""Test"",""Port"":{port}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [Test]
    public void Can_send_email_without_authentication()
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":""Test""}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }

    [TestCase( "null" )]
    [TestCase( @"""""" )]
    public void Invalid_user( string user )
    {
        var json = @$"{{""Host"":""Test"",""User"":{user},""Password"":""Test""}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [TestCase( "null" )]
    [TestCase( @"""""" )]
    public void Invalid_password( string password )
    {
        var json = @$"{{""Host"":""Test"",""User"":""Test"",""Password"":{password}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [Test]
    public void Can_send_email_with_authentication()
    {
        var json = @"{""Host"":""Test"",""User"":""Test"",""Password"":""Test""}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }

    [TestCase( nameof( SecureSocketOptions.None ) )]
    [TestCase( nameof( SecureSocketOptions.Auto ) )]
    [TestCase( nameof( SecureSocketOptions.SslOnConnect ) )]
    [TestCase( nameof( SecureSocketOptions.StartTls ) )]
    [TestCase( nameof( SecureSocketOptions.StartTlsWhenAvailable ) )]
    public void Valid_socket_options( string options )
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":""Test"",""SocketOptions"":""{options}""}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }

    [TestCase( @"""Test""" )]
    [TestCase( "3712" )]
    public void Invalid_use_ssl( string useSSl )
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":""Test"",""UseSsl"":{useSSl}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeFalse();
        sender.ShouldBeNull();
    }

    [TestCase( "true" )]
    [TestCase( "false" )]
    public void Valid_use_ssl( string useSSl )
    {
        var json = @$"{{""RequiresAuthentication"":false,""Host"":""Test"",""UseSsl"":{useSSl}}}";
        var config = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local", "Smtp", json );
        new SmtpEmailSenderFactory().TryCreateEmailSender( TestHelper.Monitor, config, out var sender ).ShouldBeTrue();
        sender.ShouldNotBeNull();
    }
}
