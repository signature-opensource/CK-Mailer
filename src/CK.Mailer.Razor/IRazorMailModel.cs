using MimeKit;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor
{
    public interface IRazorMailModel<T>
    {
        RecipientModel Recipients { get; }
        string Subject { get; }
        T Model { get; }

        MimeMessage ProcessRazorView( IRazorLightEngine engine, string template );
        MimeMessage ProcessRazorView( IRazorLightEngine engine, Func<IRazorLightEngine, string> execute );
        MimeMessage ProcessRazorString( IRazorLightEngine engine, string content );
    }
}
