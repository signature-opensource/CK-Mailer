using CK.Core;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace CK.Mailer
{
    public interface IMailerService
    {
        Task SendAsync<T>( IActivityMonitor m, MimeMessage message );
    }
}
