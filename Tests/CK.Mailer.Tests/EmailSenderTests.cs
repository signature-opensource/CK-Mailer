using CK.AppIdentity;
using CK.Mailer.MailKit;
using CK.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

[TestFixture]
public class EmailSenderTests
{
    //[Test]
    //public async Task Test_configure_Async()
    //{
    //    var config = new DynamicConfigurationSource();
    //    using( config.StartBatch() )
    //    {
    //        config["CK-AppIdentity:FullName"] = "Domain/$OneBoard/#Dev";
    //        config["CK-AppIdentity:Local:EmailSender:Smtp:0:Host"] = "test.com";
    //        config["CK-AppIdentity:Local:EmailSender:Smtp:0:Port"] = "3712";
    //        config["CK-AppIdentity:Local:EmailSender:Smtp:0:PickupDirectory"] = "/output/email";
    //        config["CK-AppIdentity:Local:EmailSender:Smtp:1:Host"] = "test2.com";
    //        config["CK-AppIdentity:Local:EmailSender:Smtp:1:Port"] = "123";
    //        config["CK-AppIdentity:Local:EmailSender:Azure:UriEndpoint"] = "endpoint.com";
    //        config["CK-AppIdentity:Local:EmailSender:Azure:Key"] = "theSecretKey";
    //    }

    //    using var app = CreateApplication( config );
    //    await app.StartAsync();

    //    var appIdentityService = app.Services.GetRequiredService<ApplicationIdentityService>();
    //    appIdentityService.FullName.Should().Be( "Domain/$OneBoard/#Dev" );
    //    appIdentityService.Remotes.Should().HaveCount( 1 );
    //}

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
        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );

        var config = builder.Configuration;
        config["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:Host"] = "test.com";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:Port"] = "3712";
        config["CK-AppIdentity:Local:EmailSender:Smtp:0:PickupDirectory"] = "";
        config["CK-AppIdentity:Local:EmailSender:Smtp:1:Host"] = "test.com";
        config["CK-AppIdentity:Local:EmailSender:Smtp:1:Port"] = "123";
        config["CK-AppIdentity:Local:EmailSender:Fake:Endpoint"] = "endpoint.com";
        config["CK-AppIdentity:Local:EmailSender:Fake:Token"] = "3712";
        config["CK-AppIdentity:Parties:0:PartyName"] = "$P1";
        config["CK-AppIdentity:Parties:0:EmailSender:Smtp:Host"] = "test3.com";
        config["CK-AppIdentity:Parties:0:EmailSender:Smtp:Port"] = "1234";

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
        await localSender.SendAsync( TestHelper.Monitor, email );

        FakeEmailSender.SentEmails.Should().HaveCount( 1 ).And.Subject.First().Should().Be( email );
    }
}
