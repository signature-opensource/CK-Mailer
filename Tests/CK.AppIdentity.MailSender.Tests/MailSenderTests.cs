using CK.Core;
using CK.IO.MailSender;
using CK.Mailer;
using CK.Mailer.MailKit;
using CK.Testing;
using CK.Testing.AppIdentity.Cris;
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

namespace CK.AppIdentity.MailSender.Tests;

// The "Test/$Sender" application is the remote: it sends its mails through the "Test/$Listener" application, its
// MailSender, that writes them in a pickup directory.
[TestFixture]
public class MailSenderTests
{
    // A dedicated port: the one of RunningApplication is used by the tests of CK-AppIdentity-Transport.
    const string Address = "tcp:127.0.0.1:37160";

    static readonly Type[] _mailerTypes = [typeof( EmailSenderFeatureDriver ),
                                           typeof( DefaultEmailSenderRegistration ),
                                           typeof( ISendMailCommand ),
                                           typeof( ISendMailResult ),
                                           typeof( IMailAddress ),
                                           typeof( IMailAttachment )];

    static readonly Type[] _remoteTypes = [.. _mailerTypes, typeof( RemoteMailSenderFeatureDriver ), typeof( MailSenderEmailSenderFactory )];

    static readonly Type[] _mailSenderTypes = [.. _mailerTypes, typeof( LocalMailSenderFeatureDriver ), typeof( MailSenderHandler ), typeof( SmtpEmailSenderFactory )];

    static NormalizedPath PickupPath => TestHelper.TestProjectFolder.AppendPart( "PickupDirectory" );

    [SetUp]
    public void CleanupPickupDirectory() => TestHelper.CleanupFolder( PickupPath );

    static Task<RunningApplicationPair> CreatePairAsync( bool remoteAllowed = true )
    {
        return RunningApplication.CreatePairAsync( TestHelper.Monitor,
            senderConfiguration: c =>
            {
                c["Parties:0:Address"] = Address;
                c["Local:EmailSender:MailSender"] = "true";
                // The MailSender: the "Local" of this party doesn't allow "MailSender".
                c["Parties:0:AllowFeatures"] = "MailSenderRemote";
            },
            senderTypes: _remoteTypes,
            listenerConfiguration: c =>
            {
                c["Defaults:ListeningAddress:0"] = Address;
                c["Local:AllowFeatures"] = "MailSender";
                c["Local:EmailSender:Smtp:SendEmail"] = "false";
                c["Local:EmailSender:Smtp:UsePickupDirectory"] = "true";
                c["Local:EmailSender:Smtp:PickupDirectory"] = PickupPath;
                if( remoteAllowed ) c["Parties:0:AllowFeatures"] = "MailSenderRemote";
            },
            listenerTypes: _mailSenderTypes );
    }

    [Test]
    [CancelAfter( 60_000 )]
    public async Task a_remote_sends_its_mail_through_its_MailSender_Async()
    {
        await using var pair = await CreatePairAsync();
        var (remote, mailSender) = pair;
        var email = new SimpleEmail { Subject = "Hello", HtmlBody = "<p>Hello <img src=\"cid:logo\"/></p>", PlaintextAlternativeBody = "Hello" }
                        .From( "intruder@test.local", "Intruder" )
                        .To( "albert@test.local", "Albert" )
                        .AddCC( "bob@test.local" );
        email.Attachments.Add( new Attachment { Filename = "readme.txt", ContentType = "text/plain", Data = new MemoryStream( Encoding.UTF8.GetBytes( "Read me." ) ) } );
        email.Attachments.Add( new Attachment { Filename = "logo.png", ContentType = "image/png", ContentId = "logo", IsInline = true, Data = new MemoryStream( [1, 2, 3] ) } );

        var response = await remote.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, email );

        response.ErrorMessages.ShouldBeEmpty();
        remote.ApplicationIdentityService.GetFeature<RemoteMailSender>().ShouldNotBeNull()
              .MailSender.PartyName.ShouldBe( "$Listener" );
        response.MessageId.ShouldNotBeNull();
        var message = await MimeMessage.LoadAsync( PickupPath.AppendPart( response.MessageId ) );
        message.Subject.ShouldBe( "Hello" );
        message.To.Mailboxes.Single().Address.ShouldBe( "albert@test.local" );
        message.Cc.Mailboxes.Single().Address.ShouldBe( "bob@test.local" );
        // The From of the remote is never sent: the MailSender uses its own (here, it has none).
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
    public async Task a_remote_that_the_MailSender_does_not_allow_is_refused_Async()
    {
        await using var pair = await CreatePairAsync( remoteAllowed: false );
        var (remote, _) = pair;

        var email = new SimpleEmail { Subject = "Hello", PlaintextAlternativeBody = "Hello" }.To( "albert@test.local" );
        var response = await remote.Services.GetRequiredService<IDefaultEmailSender>().SendAsync( TestHelper.Monitor, email );

        response.Successful.ShouldBeFalse();
        response.ErrorMessages.Single().ShouldContain( "doesn't allow the 'MailSenderRemote' feature" );
        Directory.GetFiles( PickupPath ).ShouldBeEmpty();
    }
}
