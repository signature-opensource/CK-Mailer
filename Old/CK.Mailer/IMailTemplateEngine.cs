using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Mailer
{
    public interface IMailTemplateEngine<Q> where Q : IMailTemplateKey
    {
        string GetBody<T>( T model, Q mailKey );
        bool Exist( Q mailKey );
    }
}