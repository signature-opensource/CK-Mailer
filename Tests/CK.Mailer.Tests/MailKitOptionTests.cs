using CK.Core;
using CK.Mailer.MailKit;
using MailKit.Security;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace CK.Mailer.Tests;

[TestFixture]
public class MailKitOptionTests
{
    const string Path = "CK-AppIdentity:Local:MailSender:MailKit";

    // A JSON null is not a value: "must be a value, not a section".
    static string Expected( string json, string message ) => json == "null" ? "not a section" : message;

    // Reads the configuration of the "MailKit" option with a "NoReply" sender: null on error (the errors are returned).
    static (MailKitOptionConfiguration? Model, string[] Errors) Read( string senderJson )
    {
        var section = ImmutableConfigurationSection.CreateFromJson( "CK-AppIdentity:Local:MailSender", "MailKit", $$"""{ "Senders": { "NoReply": {{senderJson}} } }""" );
        var c = new UserMessageCollector( new CurrentCultureInfo( new TranslationService(), NormalizedCultureInfo.CodeDefault ) );
        ConfigurationModelReader.TryRead<MailKitOptionConfiguration>( section, Path, c, out var m );
        return (m, c.UserMessages.Where( x => x.Level == UserMessageLevel.Error ).Select( x => x.Text ).ToArray());
    }

    static void ShouldBeValid( string json )
    {
        var (m, errors) = Read( json );
        errors.ShouldBeEmpty();
        m.ShouldNotBeNull();
        new MailKitOption( m ).Senders.Keys.ShouldBe( ["NoReply"] );
    }

    static void ShouldBeInvalid( string json, string? message = null )
    {
        var (m, errors) = Read( json );
        m.ShouldBeNull();
        errors.ShouldNotBeEmpty();
        if( message != null ) errors.ShouldContain( e => e.Contains( message ), string.Join( " | ", errors ) );
    }

    [TestCase( "\"\"" )]
    [TestCase( "3712" )]
    public void Invalid_use_pickup_directory( string directory ) => ShouldBeInvalid( $$"""{ "UsePickupDirectory": {{directory}}, "SendEmail": false }""", "UsePickupDirectory" );

    [Test]
    public void Missing_pickup_directory() => ShouldBeInvalid( """{ "UsePickupDirectory": true, "SendEmail": false }""", "PickupDirectory' is required" );

    [Test]
    public void Only_pickup_directory() => ShouldBeValid( """{ "UsePickupDirectory": true, "PickupDirectory": "Path", "SendEmail": false }""" );

    [TestCase( "true" )]
    [TestCase( "false" )]
    public void Valid_send_email( string sendEmail ) => ShouldBeValid( $$"""{ "SendEmail": {{sendEmail}}, "Host": "Test", "RequiresAuthentication": false }""" );

    [TestCase( "null" )]
    [TestCase( "\"Test\"" )]
    [TestCase( "3712" )]
    public void Invalid_send_email( string sendEmail ) => ShouldBeInvalid( $$"""{ "SendEmail": {{sendEmail}} }""", "SendEmail" );

    [Test]
    public void Missing_host() => ShouldBeInvalid( """{ "RequiresAuthentication": false }""", "Host' is required" );

    [TestCase( "null" )]
    [TestCase( "\"\"" )]
    public void Invalid_host( string host ) => ShouldBeInvalid( $$"""{ "RequiresAuthentication": false, "Host": {{host}} }""", Expected( host, "Host' is required" ) );

    [TestCase( "\"Test\"" )]
    [TestCase( "true" )]
    [TestCase( "0" )]
    public void Invalid_port( string port ) => ShouldBeInvalid( $$"""{ "RequiresAuthentication": false, "Host": "Test", "Port": {{port}} }""", "Port" );

    [Test]
    public void Can_send_email_without_authentication() => ShouldBeValid( """{ "RequiresAuthentication": false, "Host": "Test" }""" );

    [TestCase( "null" )]
    [TestCase( "\"\"" )]
    public void Invalid_user( string user ) => ShouldBeInvalid( $$"""{ "Host": "Test", "User": {{user}}, "Password": "Test" }""", Expected( user, "\"User\" and \"Password\" are required" ) );

    [TestCase( "null" )]
    [TestCase( "\"\"" )]
    public void Invalid_password( string password ) => ShouldBeInvalid( $$"""{ "Host": "Test", "User": "Test", "Password": {{password}} }""", Expected( password, "\"User\" and \"Password\" are required" ) );

    [Test]
    public void Can_send_email_with_authentication() => ShouldBeValid( """{ "Host": "Test", "User": "Test", "Password": "Test" }""" );

    [TestCase( nameof( SecureSocketOptions.None ) )]
    [TestCase( nameof( SecureSocketOptions.Auto ) )]
    [TestCase( nameof( SecureSocketOptions.SslOnConnect ) )]
    [TestCase( nameof( SecureSocketOptions.StartTls ) )]
    [TestCase( nameof( SecureSocketOptions.StartTlsWhenAvailable ) )]
    public void Valid_socket_options( string options ) => ShouldBeValid( $$"""{ "RequiresAuthentication": false, "Host": "Test", "SocketOptions": "{{options}}" }""" );

    [Test]
    public void SocketOptions_is_Auto_by_default()
    {
        var (m, _) = Read( """{ "RequiresAuthentication": false, "Host": "Test" }""" );
        new MailKitOption( m! ).SenderOptions["NoReply"].SocketOptions.ShouldBe( SecureSocketOptions.Auto );
    }

    [Test]
    public void UseSsl_is_gone_SocketOptions_replaces_it() => ShouldBeInvalid( """{ "RequiresAuthentication": false, "Host": "Test", "UseSsl": true }""", "Unknown key" );

    [Test]
    public void Invalid_from() => ShouldBeInvalid( """{ "SendEmail": false, "From": "not a mailbox" }""", "From' = 'not a mailbox'" );
}
