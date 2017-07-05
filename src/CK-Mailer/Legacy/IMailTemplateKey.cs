using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public interface IMailTemplateKey
    {
        /// <summary>
        /// A key identifiying the mail
        /// </summary>
        string Key { get; }
    }
}
