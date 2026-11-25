using CK.AppIdentity;
using System.Collections.Generic;

namespace CK.Mailer;

/// <summary>
/// Base class of the options of the "MailSender" feature that define senders: one option per sender kind ("MailKit"),
/// its configuration model holds the named senders of this kind
/// (<c>"MailKit": { "Senders": { "NoReply": {...}, "Support": {...} } }</c>). A concrete option is decorated with its
/// <see cref="PartyFeatureOptionAttribute"/> and its <c>[Configuration&lt;TModel&gt;]</c>.
/// </summary>
public abstract class MailSenderOption : IPartyComponentOption<MailSenderFeatureDriver>
{
    /// <summary>
    /// Gets the senders defined by this option, by name (see <see cref="MailSenders.IsValidSenderName(string?)"/>).
    /// </summary>
    public abstract IReadOnlyDictionary<string, IEmailSender> Senders { get; }
}
