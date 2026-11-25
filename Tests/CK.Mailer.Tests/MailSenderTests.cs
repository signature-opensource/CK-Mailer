using CK.AppIdentity;
using CK.Core;
using CK.Mailer.MailKit;
using CK.Monitoring;
using CK.Setup;
using CK.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

// The senders are MailKit senders that write their mails in a pickup directory of their own: the folder tells which
// sender has sent a mail.
[TestFixture]
public class MailSenderTests
{
    static IStObjMap? _map;

    static NormalizedPath Folder( string sender ) => TestHelper.TestProjectFolder.Combine( PickupDirectory.Path ).AppendPart( sender );

    static async Task<IStObjMap> GetMapAsync()
    {
        if( _map == null )
        {
            var configuration = TestHelper.CreateDefaultEngineConfiguration();
            // The option types are compiled in the generated code: CK.Mailer.MailKit is a head of the BinPath (the default bin path is the one of CK.Mailer).
            configuration.FirstBinPath.Path = TestHelper.BinFolder;
            configuration.FirstBinPath.Assemblies.Add( "CK.Mailer.MailKit" );
            _map = (await configuration.RunSuccessfullyAsync()).LoadMap();
        }
        return _map;
    }

    // Configures a MailKit sender that writes in its pickup folder.
    static void Sender( IConfiguration c, string prefix, string name )
    {
        Directory.CreateDirectory( Folder( name ) );
        c[$"{prefix}MailSender:MailKit:Senders:{name}:SendEmail"] = "false";
        c[$"{prefix}MailSender:MailKit:Senders:{name}:UsePickupDirectory"] = "true";
        c[$"{prefix}MailSender:MailKit:Senders:{name}:PickupDirectory"] = Folder( name );
    }

    static async Task<IHost> StartAsync( Action<IConfiguration> configure )
    {
        var map = await GetMapAsync();
        var builder = Host.CreateEmptyApplicationBuilder( new HostApplicationBuilderSettings { DisableDefaults = true } );
        builder.AddApplicationIdentityServiceConfiguration();
        builder.Configuration["CK-AppIdentity:FullName"] = "Domain/$MailerTests/#Dev";
        configure( builder.Configuration.GetSection( "CK-AppIdentity" ) );
        builder.Services.AddStObjMap( TestHelper.Monitor, map );
        var app = builder.CKBuild();
        try
        {
            await app.StartAsync();
        }
        catch
        {
            app.Dispose();
            throw;
        }
        return app;
    }

    static async Task StartMustFailAsync( string expectedError, Action<IConfiguration> configure )
    {
        using var logs = GrandOutput.Default.ShouldNotBeNull().CreateMemoryCollector( 200 );
        await Should.ThrowAsync<Exception>( () => StartAsync( configure ) );
        var all = new List<string>();
        for( int i = 0; i < 50; ++i )
        {
            all.AddRange( logs.ExtractCurrentTexts() );
            if( all.Any( t => t.Contains( expectedError ) ) ) return;
            await Task.Delay( 20 );
        }
        Assert.Fail( $"Log containing '{expectedError}' not found in:{Environment.NewLine}{string.Join( Environment.NewLine, all )}" );
    }

    static void ShouldBeSentBy( SendResponse r, string sender )
    {
        r.ErrorMessages.ShouldBeEmpty();
        File.Exists( Folder( sender ).AppendPart( r.MessageId! ) ).ShouldBeTrue();
    }

    static SimpleEmail Mail() => new SimpleEmail { Subject = "Hello", PlaintextAlternativeBody = "Hello" }.To( "albert@test.local" );

    [Test]
    public async Task a_single_sender_is_the_default_Async()
    {
        using var app = await StartAsync( c => Sender( c, "Local:", "One" ) );

        ShouldBeSentBy( await app.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() ), "One" );
    }

    [Test]
    public async Task the_Default_names_one_of_several_senders_and_the_others_are_found_by_name_Async()
    {
        using var app = await StartAsync( c =>
        {
            Sender( c, "Local:", "NoReply" );
            Sender( c, "Local:", "Support" );
            c["Local:MailSender:Default"] = "Support";
        } );
        var root = app.Services.GetRequiredService<ApplicationIdentityService>();
        root.GetRequiredFeature<MailSenders>().Names.ShouldBe( ["NoReply", "Support"], ignoreOrder: true );

        ShouldBeSentBy( await app.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() ), "Support" );
        ShouldBeSentBy( await root.GetMailSender( "NoReply" ).SendAsync( TestHelper.Monitor, Mail() ), "NoReply" );

        // An unknown sender fails the sending.
        var r = await root.GetMailSender( "Nope" ).SendAsync( TestHelper.Monitor, Mail() );
        r.Successful.ShouldBeFalse();
        r.ErrorMessages.Single().ShouldContain( "has no sender 'Nope'" );
    }

    [Test]
    public async Task a_tenant_has_its_own_senders_Async()
    {
        using var app = await StartAsync( c =>
        {
            Sender( c, "Local:", "Root" );
            c["Parties:0:FullName"] = "Acme/$Acme";
            Sender( c, "Parties:0:Local:", "Acme" );
        } );
        var root = app.Services.GetRequiredService<ApplicationIdentityService>();
        var acme = root.TenantDomains.Single();

        ShouldBeSentBy( await acme.GetMailSender().SendAsync( TestHelper.Monitor, Mail() ), "Acme" );
        ShouldBeSentBy( await app.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() ), "Root" );
    }

    [Test]
    public async Task without_MailSender_section_the_sending_fails_Async()
    {
        using var app = await StartAsync( c => { } );
        var r = await app.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() );
        r.Successful.ShouldBeFalse();
        r.ErrorMessages.Single().ShouldContain( "has no mail sender" );
    }

    [Test]
    public async Task configuration_errors_Async()
    {
        await StartMustFailAsync( "\"MailSender:Default\" must name one", c =>
        {
            Sender( c, "Local:", "A" );
            Sender( c, "Local:", "B" );
        } );
        await StartMustFailAsync( "the \"MailSender:Default\" sender 'C' is not defined", c =>
        {
            Sender( c, "Local:", "A" );
            c["Local:MailSender:Default"] = "C";
        } );
        await StartMustFailAsync( "'CK-AppIdentity:Local:MailSender:MailKit:Senders:$A': invalid sender name", c => c["Local:MailSender:MailKit:Senders:$A:SendEmail"] = "false" );
        await StartMustFailAsync( "'CK-AppIdentity:Local:MailSender:MailKit:Senders' must define at least one sender", c => c["Local:MailSender:MailKit"] = "true" );
        await StartMustFailAsync( "Unknown key 'CK-AppIdentity:Local:MailSender:MailKit:Senders:A:Hots'", c => c["Local:MailSender:MailKit:Senders:A:Hots"] = "smtp.acme.com" );
        await StartMustFailAsync( "Unknown key 'CK-AppIdentity:Local:MailSender:MailKit:A'", c => c["Local:MailSender:MailKit:A:SendEmail"] = "false" );
        await StartMustFailAsync( "a remote has no mail sender", c =>
        {
            c["Parties:0:PartyName"] = "R";
            Sender( c, "Parties:0:", "A" );
        } );
    }
}
