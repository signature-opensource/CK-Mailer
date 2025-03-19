using CK.Mailer.MailKit;
using CK.Testing;
using Shouldly;
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
        var configuration = TestHelper.CreateDefaultEngineConfiguration();
        configuration.FirstBinPath.Types.Add( typeof( FakeEmailSenderFactory ) );
        configuration.FirstBinPath.DiscoverAssembliesFromPath = true;
        var map = (await configuration.RunSuccessfullyAsync()).LoadMap();

        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );
        builder.AddApplicationIdentityServiceConfiguration();

        builder.Configuration["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Fake:Endpoint"] = "endpoint.com";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Fake:Token"] = "3712";

        builder.Services.AddStObjMap( TestHelper.Monitor, map );

        using var app = builder.CKBuild();
        await app.StartAsync();

        var localSender = app.Services.GetRequiredService<IDefaultEmailSender>();

        FakeEmailSender.SentEmails.Clear();
        var email = new SimpleEmail();
        await localSender.SendAsync( TestHelper.Monitor, email );

        FakeEmailSender.SentEmails.Count.ShouldBe( 1 );
        FakeEmailSender.SentEmails.First().ShouldBe( email );
    }

    [Test]
    public async Task configure_complete_sender_Async()
    {
        var configuration = TestHelper.CreateDefaultEngineConfiguration();
        configuration.FirstBinPath.Types.Add( typeof( FakeEmailSenderFactory ), typeof( SmtpEmailSenderFactory ) );
        configuration.FirstBinPath.DiscoverAssembliesFromPath = true;
        var map = (await configuration.RunSuccessfullyAsync()).LoadMap();

        // Needed for CI...
        Directory.CreateDirectory( PickupDirectory.Path );

        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );
        builder.AddApplicationIdentityServiceConfiguration();

        builder.Configuration["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:0:SendEmail"] = "false";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:0:UsePickupDirectory"] = "true";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:0:PickupDirectory"] = PickupDirectory.Path;
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:1:SendEmail"] = "false";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:1:UsePickupDirectory"] = "true";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Smtp:1:PickupDirectory"] = PickupDirectory.Path;
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Fake:Endpoint"] = "endpoint.com";
        builder.Configuration["CK-AppIdentity:Local:EmailSender:Fake:Token"] = "3712";
        builder.Configuration["CK-AppIdentity:Parties:0:PartyName"] = "$P1";
        builder.Configuration["CK-AppIdentity:Parties:0:EmailSender:Smtp:SendEmail"] = "false";

        builder.Services.AddStObjMap( TestHelper.Monitor, map );

        using var app = builder.CKBuild();
        await app.StartAsync();

        var localSender = app.Services.GetRequiredService<IDefaultEmailSender>();

        FakeEmailSender.SentEmails.Clear();
        var email = new SimpleEmail();
        var respons = await localSender.SendAsync( TestHelper.Monitor, email );

        respons.Successful.ShouldBeTrue();
        respons.MessageId.ShouldNotBeNullOrEmpty();
        var files = respons.MessageId!.Split( ',' );
        files.Length.ShouldBe( 2 );
        File.Exists( PickupDirectory.Path.AppendPart( files[0] ) ).ShouldBeTrue();
        File.Exists( PickupDirectory.Path.AppendPart( files[1] ) ).ShouldBeTrue();
        FakeEmailSender.SentEmails.Count.ShouldBe( 1 );
        FakeEmailSender.SentEmails.Single().ShouldBe( email );
    }
}
