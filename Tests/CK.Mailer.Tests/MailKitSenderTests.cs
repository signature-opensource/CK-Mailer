using CK.Core;
using CK.Mailer.MailKit;
using Shouldly;
using MailKit.Security;
using MimeKit;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Mailer.Tests;

[TestFixture]
public class MailKitSenderTests
{
    [Test]
    public async Task Do_nothing_Async()
    {
        var sender = new MailKitSender( new MailKitSenderOptions {SendEmail = false} );
        var response = await sender.SendAsync( TestHelper.Monitor, new SimpleEmail() );
        response.Successful.ShouldBeTrue();
        response.MessageId.ShouldBeNull();
    }

    [Test]
    public async Task Save_email_on_disk_Async()
    {
        // Needed for CI...
        Directory.CreateDirectory( PickupDirectory.Path );

        var dataPath = new NormalizedPath( "../../../Data" );
        var textFileName = "text.txt";
        var textFilePath = dataPath.AppendPart( textFileName );
        var imageFileName = "image.png";
        var imageFilePath = dataPath.AppendPart( imageFileName );

        var sender = new MailKitSender( new MailKitSenderOptions
        {
            UsePickupDirectory = true,
            PickupDirectory = PickupDirectory.Path,
            SendEmail = false
        } );

        var email = new SimpleEmail()
            .From( "sender@test.com", "Sender" )
            .To( "recipient1@test.com", "Recipient 1" )
            .AddTo( "recipient2@test.com", "Recipient 2" )
            .SetSubject( "Save an email on the disk" )
            .SetPlaintextAlternativeBody( "Content of the email." )
            .AddAttach( new Attachment
            {
                ContentId = "1",
                ContentType = "text/plain",
                Data = File.OpenRead( textFilePath ),
                Filename = textFileName
            } )
            .AddAttach( new Attachment
            {
                ContentId = "2",
                ContentType = "image/png",
                Data = File.OpenRead( imageFilePath ),
                Filename = imageFileName
            } );

        var response = await sender.SendAsync( TestHelper.Monitor, email );

        response.Successful.ShouldBeTrue();
        response.ErrorMessages.ShouldBeEmpty();
        response.MessageId.ShouldNotBeNullOrEmpty();

        var responseFile = PickupDirectory.Path.AppendPart( response.MessageId );
        File.Exists( responseFile ).ShouldBeTrue();

        var message = await MimeMessage.LoadAsync( responseFile );
        var email2 = message.GetSimpleEmail();

        email2.FromAddress.ShouldBe( email.FromAddress );
        email2.ToAddresses.ShouldBe( email.ToAddresses );
        email2.CcAddresses.ShouldBe( email.CcAddresses );
        email2.BccAddresses.ShouldBe( email.BccAddresses );
        email2.ReplyToAddresses.ShouldBe( email.ReplyToAddresses );
        email2.Subject.ShouldBe( email.Subject );
        email2.HtmlBody.ShouldBe( email.HtmlBody );
        email2.PlaintextAlternativeBody.ShouldBe( email.PlaintextAlternativeBody );
        email2.Priority.ShouldBe( email.Priority );
        email2.Attachments.Count.ShouldBe( 2 );
        email2.Attachments[0].ContentId.ShouldBe( email.Attachments[0].ContentId );
        email2.Attachments[0].Filename.ShouldBe( email.Attachments[0].Filename );
        email2.Attachments[0].ContentType.ShouldBe( email.Attachments[0].ContentType );
        email2.Attachments[0].IsInline.ShouldBe( email.Attachments[0].IsInline );
        using( var reader = new StreamReader( email2.Attachments[0].Data ) )
        {
            var email2TextContent = await reader.ReadToEndAsync();
            email2TextContent.ShouldBe( File.ReadAllText( textFilePath ) );
        }

        using( var md5 = MD5.Create() )
        {
            var emailImageHash = md5.ComputeHash( File.ReadAllBytes( imageFilePath ) );
            var email2ImageHash = md5.ComputeHash( email2.Attachments[1].Data );
            emailImageHash.ShouldBe( email2ImageHash );
        }
    }

    [Test]
    public async Task From_setting_adds_From_address_Async()
    {
        // Needed for CI...
        Directory.CreateDirectory( PickupDirectory.Path );
        var sender = new MailKitSender( new MailKitSenderOptions
        {
            UsePickupDirectory = true,
            PickupDirectory = PickupDirectory.Path,
            SendEmail = false,
            From = InternetAddress.Parse( "Hello World <hello@world.com>" )
        } );

        var email = new SimpleEmail()
            .To( "recipient1@test.com", "Recipient 1" )
            .SetSubject( "Save an email on the disk" )
            .SetPlaintextAlternativeBody( "Content of the email." );

        var response = await sender.SendAsync( TestHelper.Monitor, email );

        response.Successful.ShouldBeTrue();
        response.ErrorMessages.ShouldBeEmpty();
        response.MessageId.ShouldNotBeNullOrEmpty();

        var responseFile = PickupDirectory.Path.AppendPart( response.MessageId );
        File.Exists( responseFile ).ShouldBeTrue();

        var message = await MimeMessage.LoadAsync( responseFile );
        var email2 = message.GetSimpleEmail();

        email2.FromAddress.ToString().ShouldBe( "Hello World <hello@world.com>" );
    }
}
