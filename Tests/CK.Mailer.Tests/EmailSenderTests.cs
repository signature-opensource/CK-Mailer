using CK.AppIdentity;
using CK.Mailer.MailKit;
using CK.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

[TestFixture]
public class EmailSenderTests
{
    [Test]
    public async Task configure_fake_sender_Async()
    {
        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );

        var config = builder.Configuration;
        config["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        config["CK-AppIdentity:Local:EmailSender:Fake:Endpoint"] = "endpoint.com";
        config["CK-AppIdentity:Local:EmailSender:Fake:Token"] = "3712";

        var engine = TestHelper.CreateDefaultEngineConfiguration();
        engine.FirstBinPath.Types.Add( typeof( FakeEmailSenderFactory ),
                                       typeof( EmailSenderFeatureDriver ),
                                       typeof( ApplicationIdentityService ) );
        var engineResult = await engine.RunAsync();
        builder.Services.AddSingleton<IDefaultEmailSender>( s => s.GetRequiredService<ApplicationIdentityService>().GetRequiredFeature<DefaultEmailSender>() );
        builder.Services.AddStObjMap( TestHelper.Monitor, engineResult.FirstBinPath.LoadMap() );

        builder.AddApplicationIdentityServiceConfiguration();
        using var app = builder.CKBuild();
        await app.StartAsync();
        var identity = app.Services.GetRequiredService<ApplicationIdentityService>();
        var localSender = identity.GetRequiredFeature<IDefaultEmailSender>();

        FakeEmailSender.SentEmails.Clear();
        var email = new SimpleEmail();
        await localSender.SendAsync( TestHelper.Monitor, email );

        FakeEmailSender.SentEmails.Should().HaveCount( 1 ).And.Subject.First().Should().Be( email );
    }

    [Test]
    public async Task configure_complete_sender_Async()
    {
        Directory.CreateDirectory( PickupDirectory.Path );

        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );

        var config = builder.Configuration;
        config["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:SendEmail"] = "false";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:UsePickupDirectory"] = "true";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:PickupDirectory"] = PickupDirectory.Path;
        config["CK-AppIdentity:Local:EmailSender:Smtp:1:SendEmail"] = "false";
        config["CK-AppIdentity:Local:EmailSender:Smtp:1:UsePickupDirectory"] = "true";
        config["CK-AppIdentity:Local:EmailSender:Smtp:1:PickupDirectory"] = PickupDirectory.Path;
        config["CK-AppIdentity:Local:EmailSender:Fake:Endpoint"] = "endpoint.com";
        config["CK-AppIdentity:Local:EmailSender:Fake:Token"] = "3712";
        config["CK-AppIdentity:Parties:0:PartyName"] = "$P1";
        config["CK-AppIdentity:Parties:0:EmailSender:Smtp:SendEmail"] = "false";

        var engine = TestHelper.CreateDefaultEngineConfiguration();
        engine.FirstBinPath.Types.Add( typeof( FakeEmailSenderFactory ),
                                       typeof( SmtpEmailSenderFactory ),
                                       typeof( EmailSenderFeatureDriver ),
                                       typeof( ApplicationIdentityService ) );
        var engineResult = await engine.RunAsync();
        builder.Services.AddSingleton<IDefaultEmailSender>( s => s.GetRequiredService<ApplicationIdentityService>().GetRequiredFeature<DefaultEmailSender>() );
        builder.Services.AddStObjMap( TestHelper.Monitor, engineResult.FirstBinPath.LoadMap() );

        builder.AddApplicationIdentityServiceConfiguration();
        using var app = builder.CKBuild();
        await app.StartAsync();
        var identity = app.Services.GetRequiredService<ApplicationIdentityService>();
        var localSender = identity.GetRequiredFeature<IDefaultEmailSender>();

        FakeEmailSender.SentEmails.Clear();
        var email = new SimpleEmail();
        var respons = await localSender.SendAsync( TestHelper.Monitor, email );

        respons.Successful.Should().BeTrue();
        respons.MessageId.Should().NotBeNullOrEmpty();
        var files = respons.MessageId!.Split( ',' );
        files.Should().HaveCount( 2 );
        File.Exists( PickupDirectory.Path.AppendPart( files[0] ) ).Should().BeTrue();
        File.Exists( PickupDirectory.Path.AppendPart( files[1] ) ).Should().BeTrue();
        FakeEmailSender.SentEmails.Should().HaveCount( 1 ).And.Subject.Single().Should().Be( email );
    }
}
