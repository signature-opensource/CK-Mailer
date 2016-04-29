using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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