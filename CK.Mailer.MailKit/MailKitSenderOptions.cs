using MailKit.Security;

namespace CK.Mailer.MailKit;

public class MailKitSenderOptions
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 25;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public bool RequiresAuthentication { get; set; }
    public bool UsePickupDirectory { get; set; }
    public string MailPickupDirectory { get; set; } = string.Empty;
    public SecureSocketOptions? SocketOptions { get; set; }
}
