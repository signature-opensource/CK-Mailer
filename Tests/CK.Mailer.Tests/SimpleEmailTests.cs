using FluentAssertions;
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

        email.FromAddress.Should().Be( default( Address ) );
        email.From( "test" );
        email.FromAddress.Should().Be( new Address( "test" ) );
        email.From( "test2", "Test 2" );
        email.FromAddress.Should().Be( new Address( "test2", "Test 2" ) );
    }

    [Test]
    public void configure_email_to()
    {
        var email = new SimpleEmail();

        email.ToAddresses.Should().BeEmpty();
        email.To( "test" );
        email.ToAddresses.Should().BeEquivalentTo( [new Address( "test" )] );
        email.AddTo( "test2" );
        email.ToAddresses.Should().BeEquivalentTo( [new Address( "test" ), new Address( "test2" )] );
        email.To( "test3" );
        email.ToAddresses.Should().BeEquivalentTo( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_cc()
    {
        var email = new SimpleEmail();

        email.CcAddresses.Should().BeEmpty();
        email.CC( "test" );
        email.CcAddresses.Should().BeEquivalentTo( [new Address( "test" )] );
        email.AddCC( "test2" );
        email.CcAddresses.Should().BeEquivalentTo( [new Address( "test" ), new Address( "test2" )] );
        email.CC( "test3" );
        email.CcAddresses.Should().BeEquivalentTo( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_bcc()
    {
        var email = new SimpleEmail();

        email.BccAddresses.Should().BeEmpty();
        email.BCC( "test" );
        email.BccAddresses.Should().BeEquivalentTo( [new Address( "test" )] );
        email.AddBCC( "test2" );
        email.BccAddresses.Should().BeEquivalentTo( [new Address( "test" ), new Address( "test2" )] );
        email.BCC( "test3" );
        email.BccAddresses.Should().BeEquivalentTo( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_replyTo()
    {
        var email = new SimpleEmail();

        email.ReplyToAddresses.Should().BeEmpty();
        email.ReplyTo( "test" );
        email.ReplyToAddresses.Should().BeEquivalentTo( [new Address( "test" )] );
        email.AddReplyTo( "test2" );
        email.ReplyToAddresses.Should().BeEquivalentTo( [new Address( "test" ), new Address( "test2" )] );
        email.ReplyTo( "test3" );
        email.ReplyToAddresses.Should().BeEquivalentTo( [new Address( "test3" )] );
    }

    [Test]
    public void configure_email_subject()
    {
        var email = new SimpleEmail();

        email.Subject.Should().BeNull();
        email.SetSubject( "test" );
        email.Subject.Should().Be( "test" );
    }

    [Test]
    public void configure_email_body()
    {
        var email = new SimpleEmail();

        email.Body.Should().BeNull();
        email.SetBody( "test" );
        email.Body.Should().Be( "test" );
    }

    [Test]
    public void configure_email_isHtml()
    {
        var email = new SimpleEmail();

        email.IsHtml.Should().BeFalse();
        email.SetBody( "test", isHtml: true );
        email.IsHtml.Should().BeTrue();
    }

    [Test]
    public void configure_email_plaintextAlternativeBody()
    {
        var email = new SimpleEmail();

        email.PlaintextAlternativeBody.Should().BeNull();
        email.SetPlaintextAlternativeBody( "test" );
        email.PlaintextAlternativeBody.Should().Be( "test" );
    }

    [Test]
    public void configure_priority()
    {
        var email = new SimpleEmail();

        email.Priority.Should().Be( Priority.Normal );
        email.SetPriority( Priority.High );
        email.Priority.Should().Be( Priority.High );
    }

    [Test]
    public void configure_attachements()
    {
        var email = new SimpleEmail();

        email.Attachments.Should().BeEmpty();
        var attachement = new Attachment
        {
            ContentId = "test",
            ContentType = "test",
            Data = Stream.Null,
            Filename = "test",
            IsInline = false
        };
        email.AddAttach( attachement );
        email.Attachments.Should().BeEquivalentTo( [attachement] );
    }

    [Test]
    public void configure_tags()
    {
        var email = new SimpleEmail();

        email.Tags.Should().BeEmpty();
        email.SetTag( "test" );
        email.Tags.Should().BeEquivalentTo( ["test"] );
        email.AddTag( "test2" );
        email.Tags.Should().BeEquivalentTo( ["test", "test2"] );
        email.SetTag( "test3" );
        email.Tags.Should().BeEquivalentTo( ["test3"] );
    }

    [Test]
    public void configure_headers()
    {
        var email = new SimpleEmail();

        email.Headers.Should().BeEmpty();
        email.Header( "test", "test2" );
        email.Headers.Should().BeEquivalentTo( new Dictionary<string, string> { { "test", "test2" } } );
        email.AddHeader( "test3", "test4" );
        email.Headers.Should().BeEquivalentTo( new Dictionary<string, string> { { "test", "test2" }, { "test3", "test4" } } );
        email.Header( "test5", "test6" );
        email.Headers.Should().BeEquivalentTo( new Dictionary<string, string> { { "test5", "test6" } } );
    }
}
