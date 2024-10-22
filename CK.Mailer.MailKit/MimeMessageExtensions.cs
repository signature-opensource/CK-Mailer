using MimeKit;
using System;
using System.Linq;

namespace CK.Mailer.MailKit;

public static class MimeMessageExtensions
{
    public static SimpleEmail GetSimpleEmail( this MimeMessage message )
    {
        var email = new SimpleEmail();

        email.Subject = message.Subject;

        var from = message.From.FirstOrDefault();
        if( from is MailboxAddress fr ) email.From( fr.Address, fr.Name );
        // TODO: SimpleEmail not manage the case of many from.

        ExtractEmailFromMimeEntity( email, message.Body );

        foreach( var to in message.To )
        {
            if( to is MailboxAddress t ) email.AddTo( t.Address, t.Name );
        }
        foreach( var cc in message.Cc )
        {
            if( cc is MailboxAddress c ) email.AddCC( c.Address, c.Name );
        }
        foreach( var bcc in message.Bcc )
        {
            if( bcc is MailboxAddress b ) email.AddBCC( b.Address, b.Name );
        }
        foreach( var reply in message.ReplyTo )
        {
            if( reply is MailboxAddress r ) email.AddReplyTo( r.Address, r.Name );
        }

        email.SetPriority( message.Priority switch
        {
            MessagePriority.NonUrgent => Priority.Low,
            MessagePriority.Normal => Priority.Normal,
            MessagePriority.Urgent => Priority.High,
            _ => throw new InvalidOperationException( $"Invalid {nameof( MessagePriority )} value '{message.Priority}'." )
        } );

        foreach( var header in message.Headers )
        {
            // TODO: Mailkit adds headers Subject, From, MessageId, etc.
            // Should I filter to extract only custom headers?
            email.Headers.Add( header.Field, header.Value );
        }

        return email;
    }

    static void ExtractEmailFromMimeEntity( SimpleEmail email, MimeEntity mimeEntity )
    {
        switch( mimeEntity )
        {
            case TextPart textPart:
                if( textPart.IsAttachment )
                {
                    ExtractAttachement( textPart );
                }
                else if( textPart.IsHtml )
                {
                    email.SetHtmlBody( textPart.Text );
                }
                else if( textPart.IsPlain )
                {
                    email.SetPlaintextAlternativeBody( textPart.Text );
                }
                else
                {
                    throw new InvalidOperationException( $"Cannot extract {nameof( TextPart )} of type {mimeEntity.GetType().Name}." );
                }
                break;

            case MimePart mimePart:
                if( mimePart.IsAttachment )
                {
                    ExtractAttachement( mimePart );
                }
                else
                {
                    throw new InvalidOperationException( $"Cannot extract {nameof( MimeParser )} of type {mimeEntity.GetType().Name}." );
                }
                break;

            case MultipartAlternative alternative:
                foreach( var part in alternative )
                {
                    ExtractEmailFromMimeEntity( email, part );
                }
                break;

            case Multipart multi:
                foreach( var part in multi )
                {
                    ExtractEmailFromMimeEntity( email, part );
                }
                break;

            default:
                throw new InvalidOperationException( $"Cannot parse {nameof( MimeEntity )} of type {mimeEntity.GetType().Name}." );
        }

        void ExtractAttachement( MimePart mimePart )
        {
            email.Attachments.Add( new Attachment
            {
                Filename = mimePart.FileName,
                Data = mimePart.Content.Open(),
                ContentType = mimePart.ContentType.MimeType,
                ContentId = mimePart.ContentId
            } );
        }
    }
}
