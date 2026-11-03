using CK.Core;

namespace CK.IO.Mailer;

/// <summary>
/// An attachment of a <see cref="ISendMailCommand"/>, or an inline image of its HTML body.
/// </summary>
public interface IMailAttachment : IPoco
{
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type ("application/pdf", "image/png"...).
    /// </summary>
    string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content identifier that the HTML body references ("cid:...") for an inline attachment.
    /// </summary>
    string ContentId { get; set; }

    /// <summary>
    /// Gets or sets whether this is an inline attachment (an image of the HTML body).
    /// </summary>
    bool IsInline { get; set; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    byte[] Data { get; set; }
}
