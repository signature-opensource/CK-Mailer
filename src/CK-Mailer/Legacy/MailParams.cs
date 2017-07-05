using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public class MailParams
    {
        public RecipientModel Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public Encoding BodyEncoding { get; set; }

        public bool MustSend { get; set; }

        public MailParams()
        {
            Recipients = new RecipientModel();
            MustSend = true;
            IsBodyHtml = true;
            BodyEncoding = Encoding.UTF8;
        }
    }
}
