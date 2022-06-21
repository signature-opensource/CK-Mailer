using CK.Core;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailKitClientProvider
    {
        /// <summary>
        /// Get current Options
        /// </summary>
        MailKitOptions Options { get; }

        Task<SmtpClient> GetClientAsync( IActivityMonitor m );
    }
}
