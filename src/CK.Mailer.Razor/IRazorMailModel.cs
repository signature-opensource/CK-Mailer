using MimeKit;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor
{
    public interface IRazorMimeMessage
    {
        void SetRazorBody<T>( IRazorLightEngine engine, T model, string template );
        void SetRazorBody( IRazorLightEngine engine, Func<IRazorLightEngine, string> execute );
        void SetRazorBodyFromString<T>( IRazorLightEngine engine, T model, string content );
    }
}
