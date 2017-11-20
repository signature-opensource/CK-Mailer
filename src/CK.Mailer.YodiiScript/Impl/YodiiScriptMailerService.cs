using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer.YodiiScript.Impl
{
    public class YodiiScriptMailerService : MailerService, IYodiiScriptMailerService
    {
        public string ViewsPhysicalPath { get; set; }

        public YodiiScriptMailerService( IMailKitClientProvider provider, string viewsPhysicalPath = null )
            : base( provider )
        {
            ViewsPhysicalPath = viewsPhysicalPath;
        }
    }
}
