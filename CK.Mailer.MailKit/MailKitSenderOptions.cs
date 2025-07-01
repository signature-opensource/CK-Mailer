using MailKit.Security;
using System.Diagnostics.CodeAnalysis;
using MimeKit;

namespace CK.Mailer.MailKit;

public class MailKitSenderOptions
{
    #region Directory configurations
    [MemberNotNullWhen( true, nameof( PickupDirectory ) )]
    public bool UsePickupDirectory { get; set; } = false;

    public string? PickupDirectory { get; set; } = null;

    public InternetAddress? From { get; set; } = null;
    #endregion

    #region Sending email configurations
    [MemberNotNullWhen( true, nameof( Host ) )]
    public bool SendEmail { get; set; } = true;

    public string? Host { get; set; } = null;

    public int Port { get; set; } = 587;

    #region Authentication configurations
    [MemberNotNullWhen( true, nameof( User ), nameof( Password ) )]
    public bool RequiresAuthentication { get; set; } = true;

    public string? User { get; set; } = null;

    public string? Password { get; set; } = null;
    #endregion

    public SecureSocketOptions? SocketOptions { get; set; } = null;

    public bool UseSsl { get; set; } = true;
    #endregion
}
