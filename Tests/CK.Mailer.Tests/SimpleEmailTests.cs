using Shouldly;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace CK.Mailer.Tests;

[TestFixture]
public class SimpleEmailTests
{
    [Test]
    public void configure_email_from()
    {
        var email = new SimpleEmail();

        email.FromAddress.ShouldBeNull();
        email.From( "test" );
        email.FromAddress.ShouldBe( new Address( "test" ) );
        email.From( "test2", "Test 2" );
        email.FromAddress.ShouldBe( new Address( "test2", "Test 2" ) );
    }

    [Test]
    public void configure_email_to()
    {
        var email = new SimpleEmail();

        email.ToAddresses.ShouldBeEmpty();
        email.To( "test" );
        email.ToAddresses.ShouldBe( [new Address( "test" )] );
        email.AddTo( "test2" );
        email.ToAddresses.ShouldBe( [new Address( "test" ), new Address( "test2" )] );
        email.To( "test3" );
        email.ToAddresses.ShouldBe( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_cc()
    {
        var email = new SimpleEmail();

        email.CcAddresses.ShouldBeEmpty();
        email.CC( "test" );
        email.CcAddresses.ShouldBe( [new Address( "test" )] );
        email.AddCC( "test2" );
        email.CcAddresses.ShouldBe( [new Address( "test" ), new Address( "test2" )] );
        email.CC( "test3" );
        email.CcAddresses.ShouldBe( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_bcc()
    {
        var email = new SimpleEmail();

        email.BccAddresses.ShouldBeEmpty();
        email.BCC( "test" );
        email.BccAddresses.ShouldBe( [new Address( "test" )] );
        email.AddBCC( "test2" );
        email.BccAddresses.ShouldBe( [new Address( "test" ), new Address( "test2" )] );
        email.BCC( "test3" );
        email.BccAddresses.ShouldBe( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_replyTo()
    {
        var email = new SimpleEmail();

        email.ReplyToAddresses.ShouldBeEmpty();
        email.ReplyTo( "test" );
        email.ReplyToAddresses.ShouldBe( [new Address( "test" )] );
        email.AddReplyTo( "test2" );
        email.ReplyToAddresses.ShouldBe( [new Address( "test" ), new Address( "test2" )] );
        email.ReplyTo( "test3" );
        email.ReplyToAddresses.ShouldBe( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_subject()
    {
        var email = new SimpleEmail();

        email.Subject.ShouldBeNull();
        email.SetSubject( "test" );
        email.Subject.ShouldBe( "test" );
    }

    [Test]
    public void configure_email_html_body()
    {
        var email = new SimpleEmail();

        email.HtmlBody.ShouldBeNull();
        email.SetHtmlBody( "test" );
        email.HtmlBody.ShouldBe( "test" );
    }

    [Test]
    public void configure_email_plaintextAlternativeBody()
    {
        var email = new SimpleEmail();

        email.PlaintextAlternativeBody.ShouldBeNull();
        email.SetPlaintextAlternativeBody( "test" );
        email.PlaintextAlternativeBody.ShouldBe( "test" );
    }

    [Test]
    public void configure_priority()
    {
        var email = new SimpleEmail();

        email.Priority.ShouldBe( Priority.Normal );
        email.SetPriority( Priority.High );
        email.Priority.ShouldBe( Priority.High );
    }

    [Test]
    public void configure_attachements()
    {
        var email = new SimpleEmail();

        email.Attachments.ShouldBeEmpty();
        var attachement = new Attachment
        {
            ContentId = "test",
            ContentType = "test",
            Data = Stream.Null,
            Filename = "test",
            IsInline = false
        };
        email.AddAttach( attachement );
        email.Attachments.ShouldBe( [attachement] );
    }

    [Test]
    public void configure_headers()
    {
        var email = new SimpleEmail();

        email.Headers.ShouldBeEmpty();
        email.Header( "test", "test2" );
        email.Headers.ShouldBe( new Dictionary<string, string> { { "test", "test2" } } );
        email.AddHeader( "test3", "test4" );
        email.Headers.ShouldBe( new Dictionary<string, string> { { "test", "test2" }, { "test3", "test4" } } );
        email.Header( "test5", "test6" );
        email.Headers.ShouldBe( new Dictionary<string, string> { { "test5", "test6" } } );
    }
}
