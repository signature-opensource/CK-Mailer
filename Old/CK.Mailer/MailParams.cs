using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

        public List<Attachment> Attachments { get; private set; }

        public MailParams()
        {
            Recipients = new RecipientModel();
            Attachments = new List<Attachment>();
            MustSend = true;
            IsBodyHtml = true;
            BodyEncoding = Encoding.UTF8;
        }

    }
}