using System.Collections.Generic;

namespace CK.Mailer.Models;

public class SimpleEmail
{
    public SimpleEmail()
    {
        ToAddresses = new List<Address>();
        CcAddresses = new List<Address>();
        BccAddresses = new List<Address>();
        ReplyToAddresses = new List<Address>();
        Subject = null;
        Body = null;
        IsHtml = false;
        PlaintextAlternativeBody = null;
        Priority = Priority.Normal;
        Attachments = new List<Attachment>();
        Tags = new List<string>();
        Headers = new Dictionary<string, string>();
    }

    public Address FromAddress { get; set; }
    public IList<Address> ToAddresses { get; init; }
    public IList<Address> CcAddresses { get; init; }
    public IList<Address> BccAddresses { get; init; }
    public IList<Address> ReplyToAddresses { get; init; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsHtml { get; set; }
    public string? PlaintextAlternativeBody { get; set; }
    public Priority Priority { get; set; }
    public IList<Attachment> Attachments { get; init; }
    public IList<string> Tags { get; init; }
    public IDictionary<string, string> Headers { get; init; }

    public SimpleEmail From( string email, string? name = null )
    {
        FromAddress = new Address( email, name );
        return this;
    }

    /// <summary>
    /// Adds a recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of recipient.</param>
    /// <param name="name">Name of recipient.</param>
    /// <returns>Instance of the <see cref="SimpleEmail"/> class.</returns>
    public SimpleEmail To( string emailAddress, string? name = null )
    {
        ToAddresses.Clear();
        return AddTo( emailAddress, name );
    }

    public SimpleEmail AddTo( string email, string? name = null )
    {
        ToAddresses.Add( new Address( email, name ) );
        return this;
    }

    public SimpleEmail CC( string email, string? name = null )
    {
        CcAddresses.Clear();
        return AddCC( email, name );
    }

    public SimpleEmail AddCC( string email, string? name = null )
    {
        CcAddresses.Add( new Address( email, name ) );
        return this;
    }

    public SimpleEmail BCC( string email, string? name = null )
    {
        BccAddresses.Clear();
        return AddBCC( email, name );
    }

    public SimpleEmail AddBCC( string email, string? name = null )
    {
        BccAddresses.Add( new Address( email, name ) );
        return this;
    }

    public SimpleEmail ReplyTo( string email, string? name = null )
    {
        ReplyToAddresses.Clear();
        return AddReplyTo( email, name );
    }

    public SimpleEmail AddReplyTo( string email, string? name = null )
    {
        ReplyToAddresses.Add( new Address( email, name ) );
        return this;
    }

    public SimpleEmail SetSubject( string subject )
    {
        Subject = subject;
        return this;
    }

    public SimpleEmail SetBody( string body, bool isHtml = false )
    {
        Body = body;
        IsHtml = isHtml;
        return this;
    }

    public SimpleEmail SetPlaintextAlternativeBody( string body )
    {
        PlaintextAlternativeBody = body;
        return this;
    }

    public SimpleEmail SetPriority( Priority priority )
    {
        Priority = priority;
        return this;
    }

    public SimpleEmail AddAttach( Attachment attachement )
    {
        if( !Attachments.Contains( attachement ) )
        {
            Attachments.Add( attachement );
        }
        return this;
    }

    public SimpleEmail SetTag( string tag )
    {
        Tags.Clear();
        return AddTag( tag );
    }

    public SimpleEmail AddTag( string tag )
    {
        Tags.Add( tag );
        return this;
    }

    public SimpleEmail Header( string header, string body )
    {
        Headers.Clear();
        return AddHeader( header, body );
    }

    public SimpleEmail AddHeader( string header, string body )
    {
        Headers.Add( header, body );
        return this;
    }
}
