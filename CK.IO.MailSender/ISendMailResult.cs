using CK.Core;
using System.Collections.Generic;

namespace CK.IO.MailSender;

/// <summary>
/// The result of a <see cref="ISendMailCommand"/>: the mail has been sent when there is no error message.
/// </summary>
public interface ISendMailResult : IPoco
{
    /// <summary>
    /// Gets or sets the message identifier given by the MailSender's senders (comma separated when it has several).
    /// </summary>
    string? MessageId { get; set; }

    /// <summary>
    /// Gets the error messages. Empty on success.
    /// </summary>
    IList<string> ErrorMessages { get; }
}
