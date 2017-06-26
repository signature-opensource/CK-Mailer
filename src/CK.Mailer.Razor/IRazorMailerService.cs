using RazorLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.Razor
{
    public interface IRazorMailerService : IMailerService
    {
        IRazorLightEngine RazorEngine { get; set; }
    }
}
