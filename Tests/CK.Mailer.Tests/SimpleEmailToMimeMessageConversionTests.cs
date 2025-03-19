using System.Collections.Generic;
using CK.Mailer.MailKit;
using Shouldly;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace CK.Mailer.Tests;

[TestFixture]
public class SimpleEmailToMimeMessageConversionTests
{
    [Test]
    public void Convert_Simple_Email_to_MimeMessage()
    {
        using var stream = new MemoryStream();
        var attachementMessage = "Content of the file.";

        using( var writer = new StreamWriter( stream, Encoding.UTF8, leaveOpen: true ) )
        {
            writer.Write( attachementMessage );
            writer.Flush();
            stream.Position = 0;
        }

        var email = new SimpleEmail()
            .From( "sender@test.com", "Sender" )
            .To( "recipient1@test.com", "Recipient 1" )
            .AddTo( "recipient2@test.com", "Recipient 2" )
            .CC( "copy1@test.com", "Copy 1" )
            .AddCC( "copy2@test.com", "Copy 2" )
            .BCC( "blind-copy1@test.com", "Blind Copy 1" )
            .AddBCC( "blind-copy2@test.com", "Blind Copy 2" )
            .ReplyTo( "reply1@test.com", "Reply 1" )
            .AddReplyTo( "reply2@test.com", "Reply 2" )
            .SetSubject( "The subject of the email" )
            .SetHtmlBody( "<h1>The Html content of the email.</h1>" )
            .SetPlaintextAlternativeBody( "The alternative body of the email" )
            .SetPriority( Priority.High )
            .AddAttach( new Attachment
            {
                ContentId = "3712",
                ContentType = "text/plain",
                Filename = "file.txt",
                Data = stream
            } )
            .AddHeader( "TestHeader", "TestHeaderValue" );

        var message = email.GetMimeMessage();

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
        email2.Attachments.Count.ShouldBe( 1 );
        email2.Attachments[0].ContentId.ShouldBe( email.Attachments[0].ContentId );
        email2.Attachments[0].Filename.ShouldBe( email.Attachments[0].Filename );
        email2.Attachments[0].ContentType.ShouldBe( email.Attachments[0].ContentType );
        email2.Attachments[0].IsInline.ShouldBe( email.Attachments[0].IsInline );
        using( var reader = new StreamReader( email2.Attachments[0].Data ) )
        {
            var email2TextContent = reader.ReadToEnd();
            email2TextContent.ShouldBe( attachementMessage );
        }

        email.Headers.ShouldAll( h =>
        {
            email2.Headers.ShouldContain( h );
        } );
    }
}
