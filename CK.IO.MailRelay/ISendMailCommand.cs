using CK.Cris;
using System.Collections.Generic;
using System.ComponentModel;

namespace CK.IO.MailRelay;

/// <summary>
/// Sends a rendered mail through a "MailRelay" party: a remote party sends it to a party that is a "MailRelay" for
/// it (it declares "MailRelayRemote" on this remote). Internal to the System and executed by the receiving endpoint only (the mail may carry secrets,
/// like the link of a password reset: it never survives its reception).
/// <para>
/// The command carries the message as it is sent: the party that builds the mail renders it (its templates, its culture,
/// its branding). There is no "From": the relay sends with the configured address of its sender.
/// </para>
/// </summary>
[Internal]
public interface ISendMailCommand : ICommand<ISendMailResult>, ICommandEndpointOnly
{
    /// <summary>
    /// Gets or sets the name of the sender of the relay to use ("MailRelayRemote": { "Sender": "Support" } on the remote
    /// that is the relay). Null: the relay chooses ("MailRelay": { "Sender": ... }, else its default sender).
    /// </summary>
    string? Sender { get; set; }

    /// <summary>
    /// Gets the recipients.
    /// </summary>
    IList<IMailAddress> To { get; }

    /// <summary>
    /// Gets the carbon copy recipients.
    /// </summary>
    IList<IMailAddress> Cc { get; }

    /// <summary>
    /// Gets the blind carbon copy recipients.
    /// </summary>
    IList<IMailAddress> Bcc { get; }

    /// <summary>
    /// Gets the addresses to reply to.
    /// </summary>
    IList<IMailAddress> ReplyTo { get; }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the HTML body.
    /// </summary>
    string? HtmlBody { get; set; }

    /// <summary>
    /// Gets or sets the plain text body (the alternative of the <see cref="HtmlBody"/>, or the only body).
    /// </summary>
    string? PlaintextBody { get; set; }

    /// <summary>
    /// Gets or sets the priority. Defaults to <see cref="MailPriority.Normal"/>.
    /// </summary>
    [DefaultValue( MailPriority.Normal )]
    MailPriority Priority { get; set; }

    /// <summary>
    /// Gets the additional headers.
    /// </summary>
    IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the attachments (and the inline images of the <see cref="HtmlBody"/>).
    /// </summary>
    IList<IMailAttachment> Attachments { get; }
}
