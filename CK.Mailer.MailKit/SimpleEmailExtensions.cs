using MimeKit;
using System.Text;
using System;
using CK.Mailer.Models;
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

        message.From.Add( new MailboxAddress( email.FromAddress.Name, email.FromAddress.Email ) );

        var builder = new BodyBuilder();
        if( !string.IsNullOrEmpty( email.PlaintextAlternativeBody ) )
        {
            builder.TextBody = email.PlaintextAlternativeBody;
            builder.HtmlBody = email.Body;
        }
        else if( !email.IsHtml )
        {
            builder.TextBody = email.Body;
        }
        else
        {
            builder.HtmlBody = email.Body;
        }

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
