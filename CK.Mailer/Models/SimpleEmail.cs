using CK.Core;
using System.Collections.Generic;

namespace CK.Mailer;

public class SimpleEmail
{
    public SimpleEmail()
    {
        FromAddress = null;
        ToAddresses = [];
        CcAddresses = [];
        BccAddresses = [];
        ReplyToAddresses = [];
        Subject = null;
        HtmlBody = null;
        PlaintextAlternativeBody = null;
        Priority = Priority.Normal;
        Attachments = [];
        Headers = new Dictionary<string, string>();
    }

    public Address? FromAddress { get; set; }
    public IList<Address> ToAddresses { get; init; }
    public IList<Address> CcAddresses { get; init; }
    public IList<Address> BccAddresses { get; init; }
    public IList<Address> ReplyToAddresses { get; init; }
    public string? Subject { get; set; }
    public string? HtmlBody { get; set; }
    public string? PlaintextAlternativeBody { get; set; }
    /// <summary>
    /// Default is <see cref="Priority.Normal"/>.
    /// </summary>
    public Priority Priority { get; set; }
    public IList<Attachment> Attachments { get; init; }
    public IDictionary<string, string> Headers { get; init; }

    /// <summary>
    /// Set the sender of the email.
    /// </summary>
    /// <param name="emailAddress">Email address of the sender.</param>
    /// <param name="name">Name of the sender.</param>
    public SimpleEmail From( string emailAddress, string? name = null )
    {
        FromAddress = new Address( emailAddress, name );
        return this;
    }

    /// <summary>
    /// Remove all recipients of the email and set the new recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of recipient.</param>
    /// <param name="name">Name of recipient.</param>
    public SimpleEmail To( string emailAddress, string? name = null )
    {
        ToAddresses.Clear();
        return AddTo( emailAddress, name );
    }

    /// <summary>
    /// Adds a recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of recipient.</param>
    /// <param name="name">Name of recipient.</param>
    public SimpleEmail AddTo( string emailAddress, string? name = null )
    {
        ToAddresses.Add( new Address( emailAddress, name ) );
        return this;
    }

    /// <summary>
    /// Remove all copy recipients of the email and set the new copy recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of copy recipient.</param>
    /// <param name="name">Name of copy recipient.</param>
    public SimpleEmail CC( string emailAddress, string? name = null )
    {
        CcAddresses.Clear();
        return AddCC( emailAddress, name );
    }

    /// <summary>
    /// Adds a copy recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of copy recipient.</param>
    /// <param name="name">Name of copy recipient.</param>
    public SimpleEmail AddCC( string emailAddress, string? name = null )
    {
        CcAddresses.Add( new Address( emailAddress, name ) );
        return this;
    }

    /// <summary>
    /// Remove all blind copy recipients of the email and set the new blind copy recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of blind copy recipient.</param>
    /// <param name="name">Name of blind copy recipient.</param>
    public SimpleEmail BCC( string email, string? name = null )
    {
        BccAddresses.Clear();
        return AddBCC( email, name );
    }

    /// <summary>
    /// Adds a blind copy recipient to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of bind copy recipient.</param>
    /// <param name="name">Name of blind copy recipient.</param>
    public SimpleEmail AddBCC( string email, string? name = null )
    {
        BccAddresses.Add( new Address( email, name ) );
        return this;
    }

    /// <summary>
    /// Remove all reply of the email and set the new reply to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of reply.</param>
    /// <param name="name">Name of reply.</param>
    public SimpleEmail ReplyTo( string emailAddress, string? name = null )
    {
        ReplyToAddresses.Clear();
        return AddReplyTo( emailAddress, name );
    }

    /// <summary>
    /// Adds a reply to the email.
    /// </summary>
    /// <param name="emailAddress">Email address of reply.</param>
    /// <param name="name">Name of reply.</param>
    public SimpleEmail AddReplyTo( string emailAddress, string? name = null )
    {
        ReplyToAddresses.Add( new Address( emailAddress, name ) );
        return this;
    }

    /// <summary>
    /// Set the subject of the email.
    /// </summary>
    /// <param name="subject">Subject of the email.</param>
    public SimpleEmail SetSubject( string? subject )
    {
        Subject = subject;
        return this;
    }

    /// <summary>
    /// Set the html body of the email.
    /// </summary>
    public SimpleEmail SetHtmlBody( string? body )
    {
        HtmlBody = body;
        return this;
    }

    /// <summary>
    /// If the <see cref="HtmlBody"/> is also used, it is the alternative text, else its the main body text.
    /// </summary>
    public SimpleEmail SetPlaintextAlternativeBody( string? body )
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
        Throw.CheckNotNullArgument( attachement );
        if( !Attachments.Contains( attachement ) )
        {
            Attachments.Add( attachement );
        }
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
