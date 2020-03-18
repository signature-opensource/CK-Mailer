using CK.Core;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailKitClientProvider : IDisposable
    {
        /// <summary>
        /// Get current set Options
        /// </summary>
        MailKitOptions Options { get; }

        SmtpClient GetClient( IActivityMonitor m );

        Task<SmtpClient> GetClientAsync( IActivityMonitor m );
    }
}
