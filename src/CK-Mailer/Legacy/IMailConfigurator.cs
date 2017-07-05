using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Mailer
{
    public interface IMailConfigurator<T>
    {
        void ConfigureMail( T model, MailParams mailParams );
        string GetSubject( T Model );
    }
}
