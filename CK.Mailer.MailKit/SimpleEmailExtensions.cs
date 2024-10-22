using MimeKit;
using System.Text;
using System;
using System.Linq;

namespace CK.Mailer.MailKit;

public static class SimpleEmailExtensions
{
    public static MimeMessage GetMimeMessage( this SimpleEmail email )
    {
        var message = new MimeMessage();

        // fixes https://github.com/lukencode/FluentEmail/issues/228
        // if for any reason, subject header is not added, add it else update it.
        if( !message.Headers.Contains( HeaderId.Subject ) )
        {
            message.Headers.Add( HeaderId.Subject, Encoding.UTF8, email.Subject ?? string.Empty );
        }
        else
        {
            message.Headers[HeaderId.Subject] = email.Subject ?? string.Empty;
        }
        message.Headers.Add( HeaderId.Encoding, Encoding.UTF8.EncodingName );

        if( email.FromAddress is not null )
        {
            message.From.Add( new MailboxAddress( email.FromAddress.Value.Name, email.FromAddress.Value.Email ) );
        }

        var builder = new BodyBuilder
        {
            HtmlBody = email.HtmlBody,
            TextBody = email.PlaintextAlternativeBody
        };

        foreach( var x in email.Attachments )
        {
            var attachment = builder.Attachments.Add( x.Filename, x.Data, ContentType.Parse( x.ContentType ) );
            attachment.ContentId = x.ContentId;
        }

        message.Body = builder.ToMessageBody();

        foreach( var header in email.Headers )
        {
            message.Headers.Add( header.Key, header.Value );
        }

        message.To.AddRange( email.ToAddresses.Select( GetMailbox ) );
        message.Cc.AddRange( email.CcAddresses.Select( GetMailbox ) );
        message.Bcc.AddRange( email.BccAddresses.Select( GetMailbox ) );
        message.ReplyTo.AddRange( email.ReplyToAddresses.Select( GetMailbox ) );

        message.Priority = email.Priority switch
        {
            Priority.High => MessagePriority.Urgent,
            Priority.Normal => MessagePriority.Normal,
            Priority.Low => MessagePriority.NonUrgent,
            _ => throw new InvalidOperationException( $"Invalid {nameof( Priority )} value '{email.Priority}'." )
        };

        return message;
    }

    static MailboxAddress GetMailbox( Address address ) => new( address.Name, address.Email );
}
