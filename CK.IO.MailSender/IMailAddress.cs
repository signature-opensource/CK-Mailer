using CK.Core;

namespace CK.IO.MailSender;

/// <summary>
/// A mail address and its optional display name.
/// </summary>
public interface IMailAddress : IPoco
{
    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    string Email { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    string? Name { get; set; }
}
