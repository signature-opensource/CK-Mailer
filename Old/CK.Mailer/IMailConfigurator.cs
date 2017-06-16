using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Mailer
{
    public interface IMailConfigurator<T>
    {
        string GetSubject( T model );
        void ConfigureMail( T model, MailParams mailParams );
    }
}