namespace CK.IO.Mailer;

/// <summary>
/// The priority of a <see cref="ISendMailCommand"/>. The values are the ones of the <c>Priority</c> of CK.Mailer.
/// </summary>
public enum MailPriority : byte
{
    /// <summary>
    /// High priority.
    /// </summary>
    High = 1,

    /// <summary>
    /// Normal priority.
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Low priority.
    /// </summary>
    Low = 3
}
