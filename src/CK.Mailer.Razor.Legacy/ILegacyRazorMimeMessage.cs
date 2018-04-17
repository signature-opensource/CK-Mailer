using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor
{
    public interface ILegacyRazorMimeMessage
    {
        void SetRazorBody<T>( IPhysicalPathProvider provider, T model, string template );
        void SetRazorBodyFromString<T>( T model, string content );
    }
}
