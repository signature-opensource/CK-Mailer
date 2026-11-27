using CK.Core;
using CK.IO.MailRelay;
using CK.Mailer;
using CK.Mailer.MailKit;
using CK.Testing;
using CK.Testing.Engine.AppIdentity.Cris;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.AppIdentity.MailRelay.Tests;

// The "Test/$Sender" application is the remote: it sends its mails through the "Test/$Listener" application, its
// MailRelay, whose MailKit senders write them in their pickup directory ("NoReply" and "Support").
[TestFixture]
public class MailRelayTests
{
    // A dedicated port: the one of RunningApplication is used by the tests of CK-AppIdentity-Transport.
    const string Address = "tcp:127.0.0.1:37160";

    static readonly Type[] _mailerTypes = [typeof( MailSenderFeatureDriver ),
                                           typeof( DefaultEmailSenderRegistration ),
                                           typeof( ISendMailCommand ),
                                           typeof( ISendMailResult ),
                                           typeof( IMailAddress ),
                                           typeof( IMailAttachment )];

    static readonly Type[] _remoteTypes = [.. _mailerTypes, typeof( RemoteMailRelayFeatureDriver )];

    static readonly Type[] _relayTypes = [.. _mailerTypes, typeof( LocalMailRelayFeatureDriver ), typeof( RemoteMailRelayFeatureDriver ), typeof( MailRelayHandler ), typeof( MailKitOption )];

    static NormalizedPath Folder( string sender ) => TestHelper.TestProjectFolder.AppendPart( "PickupDirectory" ).AppendPart( sender );

    [SetUp]
    public void CleanupPickupDirectory()
    {
        TestHelper.CleanupFolder( Folder( "NoReply" ) );
        TestHelper.CleanupFolder( Folder( "Support" ) );
    }

    static void Sender( IConfiguration c, string name )
    {
        c[$"Local:MailSender:MailKit:Senders:{name}:SendEmail"] = "false";
        c[$"Local:MailSender:MailKit:Senders:{name}:UsePickupDirectory"] = "true";
        c[$"Local:MailSender:MailKit:Senders:{name}:PickupDirectory"] = Folder( name );
    }

    static Task<RunningApplicationPair> CreatePairAsync( bool remoteAllowed = true,
                                                         string? callerSender = null,
                                                         string? relaySender = null )
    {
        return RunningApplication.CreatePairAsync( TestHelper.Monitor,
            senderConfiguration: c =>
            {
                c["Parties:0:Address"] = Address;
                // The relay: the "Local" of this party doesn't declare "MailRelay".
                if( callerSender != null ) c["Parties:0:MailRelayRemote:Sender"] = callerSender;
                else c["Parties:0:MailRelayRemote"] = "true";
            },
            senderTypes: _remoteTypes,
            listenerConfiguration: c =>
            {
                c["Defaults:Transport:ListeningAddress:tcp"] = Address["tcp:".Length..];
                if( relaySender != null ) c["Local:MailRelay:Sender"] = relaySender;
                else c["Local:MailRelay"] = "true";
                Sender( c, "NoReply" );
                Sender( c, "Support" );
                c["Local:MailSender:Default"] = "NoReply";
                if( remoteAllowed ) c["Parties:0:MailRelayRemote"] = "true";
            },
            listenerTypes: _relayTypes );
    }

    static SimpleEmail Mail() => new SimpleEmail { Subject = "Hello", PlaintextAlternativeBody = "Hello" }.To( "albert@test.local" );

    static void ShouldBeSentBy( SendResponse response, string sender )
    {
        response.ErrorMessages.ShouldBeEmpty();
        File.Exists( Folder( sender ).AppendPart( response.MessageId.ShouldNotBeNull() ) ).ShouldBeTrue();
    }

    [Test]
    [CancelAfter( 60_000 )]
    public async Task a_remote_sends_its_mail_through_its_relay_Async()
    {
        await using var pair = await CreatePairAsync();
        var (remote, _) = pair;
        var email = new SimpleEmail { Subject = "Hello", HtmlBody = "<p>Hello <img src=\"cid:logo\"/></p>", PlaintextAlternativeBody = "Hello" }
                        .From( "intruder@test.local", "Intruder" )
                        .To( "albert@test.local", "Albert" )
                        .AddCC( "bob@test.local" );
        email.Attachments.Add( new Attachment { Filename = "readme.txt", ContentType = "text/plain", Data = new MemoryStream( Encoding.UTF8.GetBytes( "Read me." ) ) } );
        email.Attachments.Add( new Attachment { Filename = "logo.png", ContentType = "image/png", ContentId = "logo", IsInline = true, Data = new MemoryStream( [1, 2, 3] ) } );

        // The relay is a sender of the remote, named after it (its single sender: the default one).
        remote.ApplicationIdentityService.GetRequiredFeature<MailSenders>().Names.ShouldBe( ["$Listener"] );
        var response = await remote.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, email );

        ShouldBeSentBy( response, "NoReply" );
        var message = await MimeMessage.LoadAsync( Folder( "NoReply" ).AppendPart( response.MessageId! ) );
        message.Subject.ShouldBe( "Hello" );
        message.To.Mailboxes.Single().Address.ShouldBe( "albert@test.local" );
        message.Cc.Mailboxes.Single().Address.ShouldBe( "bob@test.local" );
        // The From of the remote is never sent: the relay uses the one of its sender (here, it has none).
        message.From.Mailboxes.ShouldBeEmpty();
        message.TextBody.ShouldBe( "Hello" );
        var attachment = message.Attachments.OfType<MimePart>().Single( a => a.FileName == "readme.txt" );
        using( var content = new MemoryStream() )
        {
            await attachment.Content.ShouldNotBeNull().DecodeToAsync( content );
            Encoding.UTF8.GetString( content.ToArray() ).ShouldBe( "Read me." );
        }
        message.BodyParts.OfType<MimePart>().ShouldContain( p => p.ContentId == "logo" );
    }

    [Test]
    [CancelAfter( 60_000 )]
    public async Task the_caller_names_the_sender_of_the_relay_Async()
    {
        await using( var pair = await CreatePairAsync( callerSender: "Support" ) )
        {
            ShouldBeSentBy( await pair.Sender.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() ), "Support" );
        }
        await using( var pair = await CreatePairAsync( callerSender: "Nope" ) )
        {
            var r = await pair.Sender.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() );
            r.Successful.ShouldBeFalse();
            r.ErrorMessages.Single().ShouldContain( "has no sender 'Nope'" );
        }
        await using( var pair = await CreatePairAsync( callerSender: "$Other" ) )
        {
            var r = await pair.Sender.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() );
            r.Successful.ShouldBeFalse();
            r.ErrorMessages.Single().ShouldContain( "doesn't relay a mail to another relay" );
        }
    }

    [Test]
    [CancelAfter( 60_000 )]
    public async Task the_relay_chooses_its_sender_when_the_caller_names_none_Async()
    {
        await using var pair = await CreatePairAsync( relaySender: "Support" );
        ShouldBeSentBy( await pair.Sender.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() ), "Support" );
    }

    [Test]
    [CancelAfter( 60_000 )]
    public async Task a_remote_that_the_relay_does_not_allow_is_refused_Async()
    {
        await using var pair = await CreatePairAsync( remoteAllowed: false );
        var (remote, _) = pair;

        var response = await remote.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, Mail() );

        response.Successful.ShouldBeFalse();
        response.ErrorMessages.Single().ShouldContain( "doesn't declare the 'MailRelayRemote' feature" );
        Directory.GetFiles( Folder( "NoReply" ) ).ShouldBeEmpty();
    }
}
