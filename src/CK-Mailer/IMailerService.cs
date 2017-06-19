using CK.Core;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailerService
    {
        IMailKitClientProvider Provider { get; set; }

        Task SendAsync( IActivityMonitor m, MimeMessage message );
    }
}
