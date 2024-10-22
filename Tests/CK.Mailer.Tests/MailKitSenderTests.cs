using CK.Core;
using CK.Mailer.MailKit;
using FluentAssertions;
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
        var sender = new MailKitSender( new MailKitSenderOptions { SendEmail = false } );
        var response = await sender.SendAsync( TestHelper.Monitor, new SimpleEmail() );
        response.Successful.Should().BeTrue();
        response.MessageId.Should().BeNull();
    }

    [Test]
    public async Task Save_email_on_disk_Async()
    {
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

        response.Successful.Should().BeTrue();
        response.ErrorMessages.Should().BeEmpty();
        response.MessageId.Should().NotBeNullOrEmpty();

        var responseFile = PickupDirectory.Path.AppendPart( response.MessageId );
        File.Exists( responseFile ).Should().BeTrue();

        var message = await MimeMessage.LoadAsync( responseFile );
        var email2 = message.GetSimpleEmail();

        email2.FromAddress.Should().BeEquivalentTo( email.FromAddress );
        email2.ToAddresses.Should().BeEquivalentTo( email.ToAddresses );
        email2.CcAddresses.Should().BeEquivalentTo( email.CcAddresses );
        email2.BccAddresses.Should().BeEquivalentTo( email.BccAddresses );
        email2.ReplyToAddresses.Should().BeEquivalentTo( email.ReplyToAddresses );
        email2.Subject.Should().Be( email.Subject );
        email2.HtmlBody.Should().Be( email.HtmlBody );
        email2.PlaintextAlternativeBody.Should().Be( email.PlaintextAlternativeBody );
        email2.Priority.Should().Be( email.Priority );
        email2.Attachments.Should().HaveSameCount( email2.Attachments ).And.HaveCount( 2 );
        email2.Attachments[0].ContentId.Should().Be( email.Attachments[0].ContentId );
        email2.Attachments[0].Filename.Should().Be( email.Attachments[0].Filename );
        email2.Attachments[0].ContentType.Should().Be( email.Attachments[0].ContentType );
        email2.Attachments[0].IsInline.Should().Be( email.Attachments[0].IsInline );
        using( var reader = new StreamReader( email2.Attachments[0].Data ) )
        {
            var email2TextContent = await reader.ReadToEndAsync();
            email2TextContent.Should().Be( File.ReadAllText( textFilePath ) );
        }
        using( var md5 = MD5.Create() )
        {
            var emailImageHash = md5.ComputeHash( File.ReadAllBytes( imageFilePath ) );
            var email2ImageHash = md5.ComputeHash( email2.Attachments[1].Data );
            emailImageHash.Should().BeEquivalentTo( email2ImageHash );
        }
    }
}
